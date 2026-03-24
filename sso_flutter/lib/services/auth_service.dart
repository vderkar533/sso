import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:http/http.dart' as http;
import 'package:http/io_client.dart';

import '../config/app_config.dart';
import '../models/auth_models.dart';
import '../models/lookup_models.dart';
import 'api_exception.dart';

class AuthService {
  final String baseUrl;
  final http.Client _client;

  AuthService({
    String? baseUrl,
    http.Client? client,
  })  : baseUrl = baseUrl ?? AppConfig.baseUrl,
        _client = client ?? _createClient();

  static http.Client _createClient() {
    final httpClient = HttpClient();
    if (AppConfig.allowBadCertificates) {
      httpClient.badCertificateCallback = (cert, host, port) => true;
    }
    return IOClient(httpClient);
  }

  Future<AuthResponse> login({
    required String email,
    required String password,
  }) async {
    final response = await _post(
      '/api/auth/login',
      {
        'email': email.trim(),
        'password': password,
        'clientId': AppConfig.clientId,
      },
    );
    return AuthResponse.fromJson(response);
  }

  Future<AuthResponse> register(RegisterRequest request) async {
    final response = await _post('/api/auth/register', request.toJson());
    return AuthResponse.fromJson(response);
  }

  Future<void> sendEmailVerification({
    required String employeeId,
    required String email,
  }) async {
    await _post('/api/auth/send-email-verification', {
      'employeeId': employeeId.trim(),
      'email': email.trim(),
    });
  }

  Future<void> verifyEmail({
    required String email,
    required String token,
  }) async {
    final uri = Uri.parse('$baseUrl/api/auth/verify-email').replace(
      queryParameters: {
        'email': email.trim(),
        'token': token.trim(),
      },
    );
    final response = await _client.get(uri, headers: {'Content-Type': 'application/json'});
    _handleResponse(response);
  }

  Future<void> forgotPassword(String email) async {
    await _post('/api/auth/forgot-password', {'email': email.trim()});
  }

  Future<void> resetPassword({
    required String email,
    required String token,
    required String newPassword,
  }) async {
    await _post('/api/auth/reset-password', {
      'email': email.trim(),
      'token': token.trim(),
      'newPassword': newPassword,
    });
  }

  Future<void> changePassword({
    required String email,
    required String currentPassword,
    required String newPassword,
  }) async {
    await _post('/api/auth/change-password', {
      'email': email.trim(),
      'currentPassword': currentPassword,
      'newPassword': newPassword,
    });
  }

  Future<AuthResponse> refreshToken(String refreshToken) async {
    final response = await _post('/api/auth/refresh-token', {
      'refreshToken': refreshToken,
      'clientId': AppConfig.clientId,
    });
    return AuthResponse.fromJson(response);
  }

  Future<void> logout(String refreshToken) async {
    await _post('/api/auth/logout', {'refreshToken': refreshToken});
  }

  Future<UserProfile> getProfile(String accessToken) async {
    final response = await _get(
      '/api/user/profile',
      accessToken: accessToken,
    );
    return UserProfile.fromJson(response);
  }

  Future<UserProfile> updateProfile({
    required UpdateProfileRequest request,
    required String accessToken,
  }) async {
    final response = await _put(
      '/api/user/profile',
      request.toJson(),
      accessToken: accessToken,
    );
    return UserProfile.fromJson(response);
  }

  Future<List<Department>> fetchDepartments() async {
    final response = await _getPublic('/api/lookups/departments');
    if (response is List) {
      return response
          .whereType<Map<String, dynamic>>()
          .map(Department.fromJson)
          .toList();
    }
    return [];
  }

  Future<List<Grade>> fetchGrades() async {
    final response = await _getPublic('/api/lookups/grades');
    if (response is List) {
      return response
          .whereType<Map<String, dynamic>>()
          .map(Grade.fromJson)
          .toList();
    }
    return [];
  }

  Future<bool> checkEmailVerified(String email) async {
    final response = await _getPublic(
      '/api/auth/debug-user?email=${Uri.encodeQueryComponent(email.trim())}',
    );
    if (response is Map<String, dynamic>) {
      return response['emailVerified'] == true;
    }
    return false;
  }

  Future<Map<String, dynamic>> _post(String path, Map<String, dynamic> body) async {
    final uri = Uri.parse('$baseUrl$path');
    try {
      final response = await _client
          .post(
            uri,
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode(body),
          )
          .timeout(AppConfig.requestTimeout);
      return _handleResponse(response) as Map<String, dynamic>;
    } on TimeoutException {
      throw ApiException(408, 'Request timed out.');
    }
  }

  Future<Map<String, dynamic>> _get(String path, {required String accessToken}) async {
    final uri = Uri.parse('$baseUrl$path');
    try {
      final response = await _client
          .get(
            uri,
            headers: {
              'Content-Type': 'application/json',
              'Authorization': 'Bearer $accessToken',
            },
          )
          .timeout(AppConfig.requestTimeout);
      return _handleResponse(response) as Map<String, dynamic>;
    } on TimeoutException {
      throw ApiException(408, 'Request timed out.');
    }
  }

  Future<Map<String, dynamic>> _put(
    String path,
    Map<String, dynamic> body, {
    required String accessToken,
  }) async {
    final uri = Uri.parse('$baseUrl$path');
    try {
      final response = await _client
          .put(
            uri,
            headers: {
              'Content-Type': 'application/json',
              'Authorization': 'Bearer $accessToken',
            },
            body: jsonEncode(body),
          )
          .timeout(AppConfig.requestTimeout);
      return _handleResponse(response) as Map<String, dynamic>;
    } on TimeoutException {
      throw ApiException(408, 'Request timed out.');
    }
  }

  Future<dynamic> _getPublic(String path) async {
    final uri = Uri.parse('$baseUrl$path');
    try {
      final response = await _client
          .get(
            uri,
            headers: {'Content-Type': 'application/json'},
          )
          .timeout(AppConfig.requestTimeout);
      return _handleResponse(response, allowList: true);
    } on TimeoutException {
      throw ApiException(408, 'Request timed out.');
    }
  }

  dynamic _handleResponse(http.Response response, {bool allowList = false}) {
    final statusCode = response.statusCode;
    final bodyText = response.body;
    if (bodyText.isEmpty) {
      if (statusCode >= 200 && statusCode < 300) {
        return allowList ? <dynamic>[] : <String, dynamic>{};
      }
      throw ApiException(statusCode, 'Empty response');
    }

    final decoded = jsonDecode(bodyText);
    if (statusCode >= 200 && statusCode < 300) {
      if (allowList && decoded is List) {
        return decoded;
      }
      if (decoded is Map<String, dynamic>) {
        return decoded;
      }
      return allowList ? <dynamic>[] : <String, dynamic>{};
    }

    if (decoded is Map<String, dynamic> && decoded['message'] != null) {
      throw ApiException(statusCode, decoded['message'].toString());
    }

    throw ApiException(statusCode, 'Request failed');
  }
}
