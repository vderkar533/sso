import 'package:flutter/material.dart';

import '../controllers/auth_controller.dart';
import '../widgets/app_widgets.dart';
import 'reset_password_screen.dart';

class ForgotPasswordScreen extends StatefulWidget {
  final AuthController controller;

  const ForgotPasswordScreen({super.key, required this.controller});

  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  final _emailController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  Future<void> _handleSend() async {
    await widget.controller.forgotPassword(_emailController.text);
    if (!mounted) return;
    if (widget.controller.errorMessage == null) {
      await showCenterMessage(
        context,
        'Reset email sent.',
        title: 'Password reset',
      );
      if (!mounted) return;
      Navigator.of(context).push(
        MaterialPageRoute(
          builder: (_) => ResetPasswordScreen(controller: widget.controller),
        ),
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
          appBar: AppBar(title: const Text('Forgot Password')),
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
                              title: 'Reset access',
                              subtitle: 'We will send a password reset link to your email.',
                            ),
                            if (widget.controller.errorMessage != null)
                              ErrorBanner(widget.controller.errorMessage!),
                            AppTextField(
                              controller: _emailController,
                              label: 'Email',
                              keyboardType: TextInputType.emailAddress,
                            ),
                            const SizedBox(height: 16),
                            PrimaryButton(
                              label: isBusy ? 'Sending...' : 'Send reset email',
                              onPressed: isBusy ? null : _handleSend,
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
