import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

class RegistrationDraft {
  final String employeeId;
  final String name;
  final String email;
  final String password;
  final int? departmentId;
  final int? gradeId;
  final String mobileNumber;
  final String aadharNumber;
  final int? userType;
  final String contractorAgencyName;
  final bool emailVerified;

  const RegistrationDraft({
    required this.employeeId,
    required this.name,
    required this.email,
    required this.password,
    required this.departmentId,
    required this.gradeId,
    required this.mobileNumber,
    required this.aadharNumber,
    required this.userType,
    required this.contractorAgencyName,
    required this.emailVerified,
  });

  RegistrationDraft copyWith({
    String? employeeId,
    String? name,
    String? email,
    String? password,
    int? departmentId,
    int? gradeId,
    String? mobileNumber,
    String? aadharNumber,
    int? userType,
    String? contractorAgencyName,
    bool? emailVerified,
  }) {
    return RegistrationDraft(
      employeeId: employeeId ?? this.employeeId,
      name: name ?? this.name,
      email: email ?? this.email,
      password: password ?? this.password,
      departmentId: departmentId ?? this.departmentId,
      gradeId: gradeId ?? this.gradeId,
      mobileNumber: mobileNumber ?? this.mobileNumber,
      aadharNumber: aadharNumber ?? this.aadharNumber,
      userType: userType ?? this.userType,
      contractorAgencyName: contractorAgencyName ?? this.contractorAgencyName,
      emailVerified: emailVerified ?? this.emailVerified,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'employeeId': employeeId,
      'name': name,
      'email': email,
      'password': password,
      'departmentId': departmentId,
      'gradeId': gradeId,
      'mobileNumber': mobileNumber,
      'aadharNumber': aadharNumber,
      'userType': userType,
      'contractorAgencyName': contractorAgencyName,
      'emailVerified': emailVerified,
    };
  }

  static RegistrationDraft fromJson(Map<String, dynamic> json) {
    return RegistrationDraft(
      employeeId: json['employeeId']?.toString() ?? '',
      name: json['name']?.toString() ?? '',
      email: json['email']?.toString() ?? '',
      password: json['password']?.toString() ?? '',
      departmentId: json['departmentId'] is num ? (json['departmentId'] as num).toInt() : null,
      gradeId: json['gradeId'] is num ? (json['gradeId'] as num).toInt() : null,
      mobileNumber: json['mobileNumber']?.toString() ?? '',
      aadharNumber: json['aadharNumber']?.toString() ?? '',
      userType: json['userType'] is num ? (json['userType'] as num).toInt() : null,
      contractorAgencyName: json['contractorAgencyName']?.toString() ?? '',
      emailVerified: json['emailVerified'] == true,
    );
  }
}

class RegistrationDraftStorage {
  static const String _key = 'registration_draft';

  static Future<void> save(RegistrationDraft draft) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_key, jsonEncode(draft.toJson()));
  }

  static Future<RegistrationDraft?> load() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_key);
    if (raw == null || raw.trim().isEmpty) {
      return null;
    }
    final decoded = jsonDecode(raw);
    if (decoded is! Map<String, dynamic>) {
      return null;
    }
    return RegistrationDraft.fromJson(decoded);
  }

  static Future<void> clear() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_key);
  }

  static Future<void> markVerified(String email) async {
    final draft = await load();
    if (draft == null) {
      if (email.trim().isEmpty) {
        return;
      }
      await save(
        RegistrationDraft(
          employeeId: '',
          name: '',
          email: email.trim(),
          password: '',
          departmentId: null,
          gradeId: null,
          mobileNumber: '',
          aadharNumber: '',
          userType: null,
          contractorAgencyName: '',
          emailVerified: true,
        ),
      );
      return;
    }

    if (email.trim().isNotEmpty &&
        draft.email.trim().isNotEmpty &&
        email.trim().toLowerCase() != draft.email.trim().toLowerCase()) {
      return;
    }

    await save(
      draft.copyWith(
        email: email.trim().isEmpty ? draft.email : email.trim(),
        emailVerified: true,
      ),
    );
  }
}
