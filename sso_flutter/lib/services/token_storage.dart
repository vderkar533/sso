import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class TokenBundle {
  final String? accessToken;
  final String? refreshToken;
  final String? idToken;
  final String? tokenType;

  const TokenBundle({
    required this.accessToken,
    required this.refreshToken,
    required this.idToken,
    required this.tokenType,
  });

  bool get hasAccessToken => accessToken != null && accessToken!.isNotEmpty;
}

class TokenStorage {
  static const _keyAccessToken = 'access_token';
  static const _keyRefreshToken = 'refresh_token';
  static const _keyIdToken = 'id_token';
  static const _keyTokenType = 'token_type';

  final FlutterSecureStorage _storage;

  TokenStorage({FlutterSecureStorage? storage}) : _storage = storage ?? const FlutterSecureStorage();

  Future<TokenBundle> readTokens() async {
    final accessToken = await _storage.read(key: _keyAccessToken);
    final refreshToken = await _storage.read(key: _keyRefreshToken);
    final idToken = await _storage.read(key: _keyIdToken);
    final tokenType = await _storage.read(key: _keyTokenType);
    return TokenBundle(
      accessToken: accessToken,
      refreshToken: refreshToken,
      idToken: idToken,
      tokenType: tokenType,
    );
  }

  Future<void> saveTokens({
    required String accessToken,
    required String refreshToken,
    required String idToken,
    required String tokenType,
  }) async {
    await _storage.write(key: _keyAccessToken, value: accessToken);
    await _storage.write(key: _keyRefreshToken, value: refreshToken);
    await _storage.write(key: _keyIdToken, value: idToken);
    await _storage.write(key: _keyTokenType, value: tokenType);
  }

  Future<void> clear() async {
    await _storage.delete(key: _keyAccessToken);
    await _storage.delete(key: _keyRefreshToken);
    await _storage.delete(key: _keyIdToken);
    await _storage.delete(key: _keyTokenType);
  }
}
