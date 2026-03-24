import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class AppTheme {
  static const Color brandBlack = Color(0xFF111111);
  static const Color brandRed = Color(0xFFC62828);
  static const Color brandRedDark = Color(0xFF8E1B1B);
  static const Color brandRedLight = Color(0xFFFFE5E5);
  static const Color brandMist = Color(0xFFF7F3F3);

  static final LinearGradient backgroundGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [
      brandMist,
      brandRedLight,
      const Color(0xFFFFF7F7),
    ],
  );

  static ThemeData light() {
    final base = ThemeData(
      colorScheme: const ColorScheme.light(
        primary: brandRed,
        onPrimary: Colors.white,
        secondary: brandRedDark,
        onSecondary: Colors.white,
        surface: Colors.white,
        onSurface: brandBlack,
        error: brandRed,
        onError: Colors.white,
      ),
      useMaterial3: true,
    );

    final textTheme = GoogleFonts.manropeTextTheme(base.textTheme)
        .apply(bodyColor: brandBlack, displayColor: brandBlack)
        .copyWith(
          titleLarge: GoogleFonts.manrope(fontWeight: FontWeight.w700),
          titleMedium: GoogleFonts.manrope(fontWeight: FontWeight.w600),
          bodyLarge: GoogleFonts.manrope(fontWeight: FontWeight.w500),
        );

    return base.copyWith(
      textTheme: textTheme,
      scaffoldBackgroundColor: brandMist,
      appBarTheme: const AppBarTheme(
        backgroundColor: brandRedLight,
        elevation: 0,
        foregroundColor: brandBlack,
        centerTitle: false,
        titleTextStyle: TextStyle(
          color: brandBlack,
          fontWeight: FontWeight.w600,
          fontSize: 16,
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: Colors.white,
        labelStyle: const TextStyle(color: brandBlack),
        hintStyle: const TextStyle(color: Colors.black54),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: BorderSide(color: brandRed.withValues(alpha: 0.35)),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: BorderSide(color: brandRed.withValues(alpha: 0.25)),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: brandRed, width: 2),
        ),
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: brandRed,
          foregroundColor: Colors.white,
          padding: const EdgeInsets.symmetric(vertical: 14),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
          textStyle: const TextStyle(fontWeight: FontWeight.w600),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(foregroundColor: brandRedDark),
      ),
      cardTheme: CardThemeData(
        color: Colors.white,
        elevation: 6,
        shadowColor: Colors.black.withValues(alpha: 0.12),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(18)),
      ),
    );
  }
}
