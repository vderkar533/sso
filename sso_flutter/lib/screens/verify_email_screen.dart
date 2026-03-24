import 'package:flutter/material.dart';

import '../controllers/auth_controller.dart';
import '../widgets/app_widgets.dart';

class VerifyEmailScreen extends StatefulWidget {
  final AuthController controller;
  final String? email;

  const VerifyEmailScreen({
    super.key,
    required this.controller,
    this.email,
  });

  @override
  State<VerifyEmailScreen> createState() => _VerifyEmailScreenState();
}

class _VerifyEmailScreenState extends State<VerifyEmailScreen> {
  late final TextEditingController _emailController;
  final _tokenController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _emailController = TextEditingController(text: widget.email ?? '');
  }

  @override
  void dispose() {
    _emailController.dispose();
    _tokenController.dispose();
    super.dispose();
  }

  Future<void> _handleVerify() async {
    await widget.controller.verifyEmail(
      _emailController.text,
      _tokenController.text,
    );
    if (!mounted) return;
    if (widget.controller.errorMessage == null) {
      await showCenterMessage(
        context,
        'Email verified.',
        title: 'Email verified',
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
          appBar: AppBar(title: const Text('Confirm Email')),
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
                              title: 'Confirm your email',
                              subtitle: 'Paste the verification token from your email.',
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
                              label: 'Verification token',
                            ),
                            const SizedBox(height: 16),
                            PrimaryButton(
                              label: isBusy ? 'Verifying...' : 'Verify',
                              onPressed: isBusy ? null : _handleVerify,
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
