import 'package:flutter/material.dart';

import '../config/app_config.dart';
import '../controllers/auth_controller.dart';
import '../widgets/app_widgets.dart';
import 'forgot_password_screen.dart';
import 'register_screen.dart';

class LoginScreen extends StatefulWidget {
  final AuthController controller;

  const LoginScreen({super.key, required this.controller});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _showPassword = false;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _handleLogin() async {
    await widget.controller.login(
      _emailController.text,
      _passwordController.text,
    );
    if (!mounted) return;
    if (widget.controller.errorMessage == null && widget.controller.isAuthenticated) {
      await showCenterMessage(
        context,
        'Login successful.',
        title: 'Welcome',
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: widget.controller,
      builder: (context, _) {
        final isBusy = widget.controller.isBusy;
        return Scaffold(
          appBar: AppBar(
            title: const Text(
              AppConfig.appName,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
            ),
            toolbarHeight: 72,
          ),
          body: AppBackground(
            child: SafeArea(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(20),
                child: Center(
                  child: ResponsiveMaxWidth(
                    child: Card(
                      child: Padding(
                        padding: const EdgeInsets.all(20),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            const BrandHeader(
                              title: 'Welcome back',
                              subtitle: 'Sign in to access your SSO profile.',
                            ),
                            if (widget.controller.errorMessage != null)
                              ErrorBanner(widget.controller.errorMessage!),
                            AppTextField(
                              controller: _emailController,
                              label: 'Email',
                              keyboardType: TextInputType.emailAddress,
                            ),
                            const SizedBox(height: 12),
                            AppTextField(
                              controller: _passwordController,
                              label: 'Password',
                              obscureText: !_showPassword,
                              suffixIcon: IconButton(
                                onPressed: () => setState(() => _showPassword = !_showPassword),
                                icon: Icon(_showPassword ? Icons.visibility_off : Icons.visibility),
                              ),
                            ),
                            const SizedBox(height: 16),
                            PrimaryButton(
                              label: isBusy ? 'Signing in...' : 'Login',
                              onPressed: isBusy ? null : _handleLogin,
                            ),
                            const SizedBox(height: 12),
                            TextButton(
                              onPressed: isBusy
                                  ? null
                                  : () {
                                      Navigator.of(context).push(
                                        MaterialPageRoute(
                                          builder: (_) => RegisterScreen(controller: widget.controller),
                                        ),
                                      );
                                    },
                              child: const Text('Create account'),
                            ),
                            TextButton(
                              onPressed: isBusy
                                  ? null
                                  : () {
                                      Navigator.of(context).push(
                                        MaterialPageRoute(
                                          builder: (_) => ForgotPasswordScreen(controller: widget.controller),
                                        ),
                                      );
                                    },
                              child: const Text('Forgot password'),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        );
      },
    );
  }
}
