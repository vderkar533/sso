import 'package:flutter/material.dart';

import '../controllers/auth_controller.dart';
import '../widgets/app_widgets.dart';

class ResetPasswordScreen extends StatefulWidget {
  final AuthController controller;

  const ResetPasswordScreen({super.key, required this.controller});

  @override
  State<ResetPasswordScreen> createState() => _ResetPasswordScreenState();
}

class _ResetPasswordScreenState extends State<ResetPasswordScreen> {
  final _emailController = TextEditingController();
  final _tokenController = TextEditingController();
  final _newPasswordController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    _tokenController.dispose();
    _newPasswordController.dispose();
    super.dispose();
  }

  Future<void> _handleReset() async {
    await widget.controller.resetPassword(
      _emailController.text,
      _tokenController.text,
      _newPasswordController.text,
    );
    if (!mounted) return;
    if (widget.controller.errorMessage == null) {
      await showCenterMessage(
        context,
        'Password reset successful.',
        title: 'Password reset',
      );
      if (!mounted) return;
      Navigator.of(context).pop();
    }
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: widget.controller,
      builder: (context, _) {
        final isBusy = widget.controller.isBusy;
        return Scaffold(
          appBar: AppBar(title: const Text('Reset Password')),
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
                              title: 'Create a new password',
                              subtitle: 'Enter the token you received by email.',
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
                              controller: _tokenController,
                              label: 'Reset token',
                            ),
                            const SizedBox(height: 12),
                            AppTextField(
                              controller: _newPasswordController,
                              label: 'New password',
                              obscureText: true,
                            ),
                            const SizedBox(height: 16),
                            PrimaryButton(
                              label: isBusy ? 'Resetting...' : 'Reset password',
                              onPressed: isBusy ? null : _handleReset,
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
