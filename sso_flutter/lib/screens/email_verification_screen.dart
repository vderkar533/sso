import 'package:flutter/material.dart';

import '../controllers/auth_controller.dart';
import '../widgets/app_widgets.dart';
import 'verify_email_screen.dart';

class EmailVerificationScreen extends StatefulWidget {
  final AuthController controller;

  const EmailVerificationScreen({super.key, required this.controller});

  @override
  State<EmailVerificationScreen> createState() => _EmailVerificationScreenState();
}

class _EmailVerificationScreenState extends State<EmailVerificationScreen> {
  final _employeeIdController = TextEditingController();
  final _emailController = TextEditingController();

  @override
  void dispose() {
    _employeeIdController.dispose();
    _emailController.dispose();
    super.dispose();
  }

  Future<void> _handleSend() async {
    await widget.controller.sendEmailVerification(
      _employeeIdController.text,
      _emailController.text,
    );
    if (!mounted) return;
    if (widget.controller.errorMessage == null) {
      await showCenterMessage(
        context,
        'Verification email sent.',
        title: 'Email verification',
      );
      if (!mounted) return;
      Navigator.of(context).push(
        MaterialPageRoute(
          builder: (_) => VerifyEmailScreen(
            controller: widget.controller,
            email: _emailController.text.trim(),
          ),
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
          appBar: AppBar(title: const Text('Verify Email')),
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
                              title: 'Email verification',
                              subtitle: 'Send a verification link to your work email.',
                            ),
                            if (widget.controller.errorMessage != null)
                              ErrorBanner(widget.controller.errorMessage!),
                            AppTextField(
                              controller: _employeeIdController,
                              label: 'Employee ID',
                            ),
                            const SizedBox(height: 12),
                            AppTextField(
                              controller: _emailController,
                              label: 'Email (@lloyds.in)',
                              keyboardType: TextInputType.emailAddress,
                            ),
                            const SizedBox(height: 16),
                            PrimaryButton(
                              label: isBusy ? 'Sending...' : 'Send verification email',
                              onPressed: isBusy ? null : _handleSend,
                            ),
                            const SizedBox(height: 8),
                            Text(
                              'After you receive the email, open the link or paste the token on the next screen.',
                              style: Theme.of(context).textTheme.bodySmall?.copyWith(color: Colors.black54),
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
