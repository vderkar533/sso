import 'package:flutter/material.dart';

import '../theme/app_theme.dart';

class AppBackground extends StatelessWidget {
  final Widget child;

  const AppBackground({super.key, required this.child});

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(gradient: AppTheme.backgroundGradient),
      child: child,
    );
  }
}

class ResponsiveMaxWidth extends StatelessWidget {
  final Widget child;
  final double mobile;
  final double tablet;
  final double desktop;
  final double wide;

  const ResponsiveMaxWidth({
    super.key,
    required this.child,
    this.mobile = 520,
    this.tablet = 720,
    this.desktop = 960,
    this.wide = 1100,
  });

  double _targetWidth(double screenWidth) {
    if (screenWidth >= 1400) return wide;
    if (screenWidth >= 1100) return desktop;
    if (screenWidth >= 800) return tablet;
    return mobile;
  }

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final screenWidth = constraints.maxWidth;
        final targetWidth = _targetWidth(screenWidth);
        final maxWidth = screenWidth < targetWidth ? screenWidth : targetWidth;
        return ConstrainedBox(
          constraints: BoxConstraints(maxWidth: maxWidth),
          child: child,
        );
      },
    );
  }
}

class ResponsiveTwoColumn extends StatelessWidget {
  final Widget left;
  final Widget right;
  final double breakpoint;
  final double gap;

  const ResponsiveTwoColumn({
    super.key,
    required this.left,
    required this.right,
    this.breakpoint = 900,
    this.gap = 16,
  });

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final isWide = constraints.maxWidth >= breakpoint;
        if (!isWide) {
          return Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              left,
              SizedBox(height: gap),
              right,
            ],
          );
        }
        return Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(child: left),
            SizedBox(width: gap),
            Expanded(child: right),
          ],
        );
      },
    );
  }
}

class BrandHeader extends StatelessWidget {
  final String title;
  final String subtitle;
  final bool onDark;

  const BrandHeader({
    super.key,
    required this.title,
    required this.subtitle,
    this.onDark = false,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            title,
            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                  color: onDark ? Colors.white : AppTheme.brandBlack,
                  fontWeight: FontWeight.w700,
                ),
          ),
          const SizedBox(height: 6),
          Text(
            subtitle,
            style: Theme.of(context)
                .textTheme
                .bodyMedium
                ?.copyWith(color: onDark ? Colors.white70 : Colors.black54),
          ),
        ],
      ),
    );
  }
}

class AppTextField extends StatelessWidget {
  final TextEditingController controller;
  final String label;
  final bool obscureText;
  final TextInputType keyboardType;
  final int? maxLength;
  final bool readOnly;
  final bool enabled;
  final Widget? suffixIcon;

  const AppTextField({
    super.key,
    required this.controller,
    required this.label,
    this.obscureText = false,
    this.keyboardType = TextInputType.text,
    this.maxLength,
    this.readOnly = false,
    this.enabled = true,
    this.suffixIcon,
  });

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: controller,
      obscureText: obscureText,
      keyboardType: keyboardType,
      maxLength: maxLength,
      readOnly: readOnly,
      enabled: enabled,
      style: const TextStyle(color: AppTheme.brandBlack),
      decoration: InputDecoration(
        labelText: label,
        suffixIcon: suffixIcon,
      ),
    );
  }
}

class PrimaryButton extends StatelessWidget {
  final String label;
  final VoidCallback? onPressed;

  const PrimaryButton({
    super.key,
    required this.label,
    required this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: double.infinity,
      child: ElevatedButton(
        onPressed: onPressed,
        child: Text(label),
      ),
    );
  }
}

class AppDropdownField<T> extends StatelessWidget {
  final T? initialValue;
  final List<DropdownMenuItem<T>> items;
  final ValueChanged<T?>? onChanged;
  final String label;
  final String? hint;

  const AppDropdownField({
    super.key,
    required this.items,
    required this.onChanged,
    required this.label,
    this.initialValue,
    this.hint,
  });

  @override
  Widget build(BuildContext context) {
    return DropdownButtonFormField<T>(
      initialValue: initialValue,
      items: items,
      onChanged: onChanged,
      isExpanded: true,
      dropdownColor: Colors.white,
      iconEnabledColor: AppTheme.brandRedDark,
      style: const TextStyle(color: AppTheme.brandBlack),
      hint: hint == null
          ? null
          : Text(
              hint!,
              style: const TextStyle(color: Colors.black54),
            ),
      decoration: InputDecoration(labelText: label),
    );
  }
}

class SectionTitle extends StatelessWidget {
  final String text;

  const SectionTitle(this.text, {super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Text(
        text,
        style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w600),
      ),
    );
  }
}

Future<void> showCenterMessage(
  BuildContext context,
  String message, {
  String title = 'Message',
}) {
  return showDialog<void>(
    context: context,
    builder: (context) => AlertDialog(
      title: Text(title),
      content: Text(message),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('OK'),
        ),
      ],
    ),
  );
}

class ErrorBanner extends StatelessWidget {
  final String message;

  const ErrorBanner(this.message, {super.key});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.red.shade50,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: Colors.red.shade200),
      ),
      child: Text(
        message,
        style: TextStyle(color: Colors.red.shade900),
      ),
    );
  }
}
