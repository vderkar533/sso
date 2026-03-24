import 'dart:async';

import 'package:app_links/app_links.dart';
import 'package:flutter/material.dart';

import 'config/app_config.dart';
import 'controllers/auth_controller.dart';
import 'screens/login_screen.dart';
import 'screens/profile_screen.dart';
import 'screens/register_screen.dart';
import 'services/auth_service.dart';
import 'services/registration_draft_storage.dart';
import 'services/token_storage.dart';
import 'theme/app_theme.dart';
import 'widgets/app_widgets.dart';

void main() {
  runApp(const SsoApp());
}

class SsoApp extends StatefulWidget {
  const SsoApp({super.key});

  @override
  State<SsoApp> createState() => _SsoAppState();
}

class _SsoAppState extends State<SsoApp> {
  late final AuthController _controller;
  final GlobalKey<NavigatorState> _navigatorKey = GlobalKey<NavigatorState>();
  AppLinks? _appLinks;
  StreamSubscription<Uri>? _linkSub;

  @override
  void initState() {
    super.initState();
    final authService = AuthService(baseUrl: AppConfig.baseUrl);
    final storage = TokenStorage();
    _controller = AuthController(authService, storage);
    _controller.initialize();
    _initDeepLinks();
  }

  @override
  void dispose() {
    _linkSub?.cancel();
    _appLinks = null;
    _controller.dispose();
    super.dispose();
  }

  Future<void> _initDeepLinks() async {
    _appLinks = AppLinks();

    final initial = await _appLinks!.getInitialLink();
    if (initial != null) {
      await _handleDeepLink(initial);
    }

    _linkSub = _appLinks!.uriLinkStream.listen(_handleDeepLink);
  }

  Future<void> _handleDeepLink(Uri uri) async {
    if (uri.scheme != 'lloyds-sso' || uri.host != 'verify') {
      return;
    }

    final email = uri.queryParameters['email'] ?? '';
    await RegistrationDraftStorage.markVerified(email);

    if (!mounted) {
      return;
    }

    final navigatorState = _navigatorKey.currentState;
    if (navigatorState == null) {
      return;
    }

    navigatorState.push(
      MaterialPageRoute(
        builder: (_) => RegisterScreen(controller: _controller),
      ),
    );

    await showCenterMessage(
      navigatorState.context,
      'Email verified. Continue registration.',
      title: 'Email verified',
    );
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, _) {
        return MaterialApp(
          navigatorKey: _navigatorKey,
          title: AppConfig.appName,
          theme: AppTheme.light(),
          home: _controller.isAuthenticated
              ? ProfileScreen(controller: _controller)
              : LoginScreen(controller: _controller),
        );
      },
    );
  }
}
