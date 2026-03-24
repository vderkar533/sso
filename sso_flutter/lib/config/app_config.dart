class AppConfig {
  const AppConfig._();

  // Update this to your server base URL.
  // For Android emulator, localhost is 10.0.2.2.
  static const String baseUrl = 'http://10.0.2.2:5023';

  // Matches the backend client id in your POSTMAN examples.
  static const String clientId = 'internal-portal';

  // Enable for local dev if using self-signed HTTPS.
  static const bool allowBadCertificates = true;

  static const String appName = 'LLoyds Metals and Energy Ltd. SSO System';

  static const Duration requestTimeout = Duration(seconds: 20);
}
