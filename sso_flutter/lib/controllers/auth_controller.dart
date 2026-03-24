import 'package:flutter/foundation.dart';

import '../models/auth_models.dart';
import '../models/lookup_models.dart';
import '../services/api_exception.dart';
import '../services/auth_service.dart';
import '../services/token_storage.dart';

class AuthController extends ChangeNotifier {
  final AuthService _authService;
  final TokenStorage _tokenStorage;

  bool isBusy = false;
  String? errorMessage;
  AuthUserInfo? userInfo;
  UserProfile? profile;
  List<Department> departments = [];
  List<Grade> grades = [];

  TokenBundle _tokens = const TokenBundle(
    accessToken: null,
    refreshToken: null,
    idToken: null,
    tokenType: null,
  );

  AuthController(this._authService, this._tokenStorage);

  bool get isAuthenticated => _tokens.hasAccessToken;

  Future<void> initialize() async {
    isBusy = true;
    errorMessage = null;
    notifyListeners();

    try {
      _tokens = await _tokenStorage.readTokens();
      if (_tokens.hasAccessToken) {
        await _loadProfileWithRefresh();
      }
    } on ApiException catch (ex) {
      errorMessage = ex.message;
    } catch (ex) {
      errorMessage = ex.toString();
    }

    isBusy = false;
    notifyListeners();
  }

  Future<void> login(String email, String password) async {
    await _runAuthFlow(() async {
      final response = await _authService.login(email: email, password: password);
      await _saveTokens(response);
      userInfo = response.user;
      await _loadProfileWithRefresh();
    });
  }

  Future<void> register(RegisterRequest request) async {
    await _runAuthFlow(() async {
      await _authService.register(request);
    });
  }

  Future<void> sendEmailVerification(String employeeId, String email) async {
    await _runAuthFlow(() async {
      await _authService.sendEmailVerification(
        employeeId: employeeId,
        email: email,
      );
    });
  }

  Future<void> verifyEmail(String email, String token) async {
    await _runAuthFlow(() async {
      await _authService.verifyEmail(email: email, token: token);
    });
  }

  Future<void> forgotPassword(String email) async {
    await _runAuthFlow(() async {
      await _authService.forgotPassword(email);
    });
  }

  Future<void> resetPassword(String email, String token, String newPassword) async {
    await _runAuthFlow(() async {
      await _authService.resetPassword(
        email: email,
        token: token,
        newPassword: newPassword,
      );
    });
  }

  Future<bool> changePassword(String email, String currentPassword, String newPassword) async {
    isBusy = true;
    errorMessage = null;
    notifyListeners();

    try {
      await _authService.changePassword(
        email: email,
        currentPassword: currentPassword,
        newPassword: newPassword,
      );
      return true;
    } on ApiException catch (ex) {
      errorMessage = ex.message;
    } catch (ex) {
      errorMessage = ex.toString();
    } finally {
      isBusy = false;
      notifyListeners();
    }

    return false;
  }

  Future<void> refreshProfile() async {
    await _runAuthFlow(() async {
      await _loadProfileWithRefresh();
    });
  }

  Future<bool> updateProfile(UpdateProfileRequest request) async {
    final accessToken = _tokens.accessToken;
    if (accessToken == null || accessToken.isEmpty) {
      errorMessage = 'Missing access token.';
      notifyListeners();
      return false;
    }

    isBusy = true;
    errorMessage = null;
    notifyListeners();

    try {
      profile = await _authService.updateProfile(
        request: request,
        accessToken: accessToken,
      );
      return true;
    } on ApiException catch (ex) {
      errorMessage = ex.message;
    } catch (ex) {
      errorMessage = ex.toString();
    } finally {
      isBusy = false;
      notifyListeners();
    }

    return false;
  }

  Future<bool> checkEmailVerified(String email) async {
    isBusy = true;
    errorMessage = null;
    notifyListeners();

    try {
      return await _authService.checkEmailVerified(email);
    } on ApiException catch (ex) {
      errorMessage = ex.message;
    } catch (ex) {
      errorMessage = ex.toString();
    } finally {
      isBusy = false;
      notifyListeners();
    }

    return false;
  }

  Future<void> logout() async {
    await _runAuthFlow(() async {
      if ((_tokens.refreshToken ?? '').isNotEmpty) {
        try {
          await _authService.logout(_tokens.refreshToken!);
        } catch (_) {
          // Ignore server-side logout errors.
        }
      }
      await _tokenStorage.clear();
      _tokens = const TokenBundle(
        accessToken: null,
        refreshToken: null,
        idToken: null,
        tokenType: null,
      );
      userInfo = null;
      profile = null;
    });
  }

  Future<void> loadLookups({bool force = false}) async {
    if (!force && departments.isNotEmpty && grades.isNotEmpty) {
      return;
    }
    await _runAuthFlow(() async {
      departments = await _authService.fetchDepartments();
      grades = await _authService.fetchGrades();
    });
  }

  Future<void> _saveTokens(AuthResponse response) async {
    _tokens = TokenBundle(
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      idToken: response.idToken,
      tokenType: response.tokenType,
    );
    await _tokenStorage.saveTokens(
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      idToken: response.idToken,
      tokenType: response.tokenType,
    );
  }

  Future<void> _loadProfileWithRefresh() async {
    final accessToken = _tokens.accessToken;
    if (accessToken == null || accessToken.isEmpty) {
      profile = null;
      return;
    }

    try {
      profile = await _authService.getProfile(accessToken);
    } on ApiException catch (ex) {
      if (ex.statusCode == 401 && (_tokens.refreshToken ?? '').isNotEmpty) {
        final refreshed = await _authService.refreshToken(_tokens.refreshToken!);
        await _saveTokens(refreshed);
        profile = await _authService.getProfile(_tokens.accessToken ?? '');
      } else {
        rethrow;
      }
    }
  }

  Future<void> _runAuthFlow(Future<void> Function() action) async {
    isBusy = true;
    errorMessage = null;
    notifyListeners();

    try {
      await action();
    } on ApiException catch (ex) {
      errorMessage = ex.message;
    } catch (ex) {
      errorMessage = ex.toString();
    } finally {
      isBusy = false;
      notifyListeners();
    }
  }
}
