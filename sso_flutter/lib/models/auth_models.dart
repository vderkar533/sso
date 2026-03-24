class AuthUserInfo {
  final String userId;
  final String employeeId;
  final String name;
  final String email;
  final int? departmentId;
  final String departmentName;
  final int? gradeId;
  final String gradeCode;
  final String gradeTitle;
  final String mobileNumber;
  final String aadharNumber;
  final String userType;
  final bool emailVerified;
  final String contractorAgencyName;

  AuthUserInfo({
    required this.userId,
    required this.employeeId,
    required this.name,
    required this.email,
    required this.departmentId,
    required this.departmentName,
    required this.gradeId,
    required this.gradeCode,
    required this.gradeTitle,
    required this.mobileNumber,
    required this.aadharNumber,
    required this.userType,
    required this.emailVerified,
    required this.contractorAgencyName,
  });

  factory AuthUserInfo.fromJson(Map<String, dynamic> json) {
    return AuthUserInfo(
      userId: json['userId']?.toString() ?? '',
      employeeId: json['employeeId']?.toString() ?? '',
      name: json['name']?.toString() ?? '',
      email: json['email']?.toString() ?? '',
      departmentId: json['departmentId'] is num ? (json['departmentId'] as num).toInt() : null,
      departmentName: json['departmentName']?.toString() ?? '',
      gradeId: json['gradeId'] is num ? (json['gradeId'] as num).toInt() : null,
      gradeCode: json['gradeCode']?.toString() ?? '',
      gradeTitle: json['gradeTitle']?.toString() ?? '',
      mobileNumber: json['mobileNumber']?.toString() ?? '',
      aadharNumber: json['aadharNumber']?.toString() ?? '',
      userType: json['userType']?.toString() ?? '',
      emailVerified: json['emailVerified'] == true,
      contractorAgencyName: json['contractorAgencyName']?.toString() ?? '',
    );
  }
}

class AuthResponse {
  final String accessToken;
  final String refreshToken;
  final String idToken;
  final int expiresIn;
  final String tokenType;
  final AuthUserInfo? user;

  AuthResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.idToken,
    required this.expiresIn,
    required this.tokenType,
    required this.user,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      accessToken: json['accessToken']?.toString() ?? '',
      refreshToken: json['refreshToken']?.toString() ?? '',
      idToken: json['idToken']?.toString() ?? '',
      expiresIn: json['expiresIn'] is num ? (json['expiresIn'] as num).toInt() : 0,
      tokenType: json['tokenType']?.toString() ?? 'Bearer',
      user: json['user'] is Map<String, dynamic> ? AuthUserInfo.fromJson(json['user'] as Map<String, dynamic>) : null,
    );
  }
}

class UserProfile {
  final String userId;
  final String employeeId;
  final String name;
  final String email;
  final int? departmentId;
  final String department;
  final int? gradeId;
  final String grade;
  final String gradeTitle;
  final int hierarchyLevel;
  final String userType;
  final int userTypeId;
  final String mobileNumber;
  final String aadharNumber;
  final String contractorAgencyName;

  UserProfile({
    required this.userId,
    required this.employeeId,
    required this.name,
    required this.email,
    required this.departmentId,
    required this.department,
    required this.gradeId,
    required this.grade,
    required this.gradeTitle,
    required this.hierarchyLevel,
    required this.userType,
    required this.userTypeId,
    required this.mobileNumber,
    required this.aadharNumber,
    required this.contractorAgencyName,
  });

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      userId: json['userId']?.toString() ?? '',
      employeeId: json['employeeId']?.toString() ?? '',
      name: json['name']?.toString() ?? '',
      email: json['email']?.toString() ?? '',
      departmentId: json['departmentId'] is num ? (json['departmentId'] as num).toInt() : null,
      department: json['department']?.toString() ?? '',
      gradeId: json['gradeId'] is num ? (json['gradeId'] as num).toInt() : null,
      grade: json['grade']?.toString() ?? '',
      gradeTitle: json['gradeTitle']?.toString() ?? '',
      hierarchyLevel: json['hierarchyLevel'] is num ? (json['hierarchyLevel'] as num).toInt() : 0,
      userType: json['userType']?.toString() ?? '',
      userTypeId: json['userTypeId'] is num ? (json['userTypeId'] as num).toInt() : 0,
      mobileNumber: json['mobileNumber']?.toString() ?? '',
      aadharNumber: json['aadharNumber']?.toString() ?? '',
      contractorAgencyName: json['contractorAgencyName']?.toString() ?? '',
    );
  }
}

class UpdateProfileRequest {
  final String employeeId;
  final String name;
  final int departmentId;
  final int gradeId;
  final String mobileNumber;
  final String aadharNumber;
  final int userType;
  final String? contractorAgencyName;

  UpdateProfileRequest({
    required this.employeeId,
    required this.name,
    required this.departmentId,
    required this.gradeId,
    required this.mobileNumber,
    required this.aadharNumber,
    required this.userType,
    this.contractorAgencyName,
  });

  Map<String, dynamic> toJson() {
    return {
      'employeeId': employeeId,
      'name': name,
      'departmentId': departmentId,
      'gradeId': gradeId,
      'mobileNumber': mobileNumber,
      'aadharNumber': aadharNumber,
      'userType': userType,
      if (contractorAgencyName != null && contractorAgencyName!.trim().isNotEmpty)
        'contractorAgencyName': contractorAgencyName,
    };
  }
}

class RegisterRequest {
  final String employeeId;
  final String name;
  final String email;
  final String password;
  final int departmentId;
  final int gradeId;
  final String mobileNumber;
  final String aadharNumber;
  final int userType;
  final String? contractorAgencyName;

  RegisterRequest({
    required this.employeeId,
    required this.name,
    required this.email,
    required this.password,
    required this.departmentId,
    required this.gradeId,
    required this.mobileNumber,
    required this.aadharNumber,
    required this.userType,
    this.contractorAgencyName,
  });

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
      if (contractorAgencyName != null && contractorAgencyName!.trim().isNotEmpty)
        'contractorAgencyName': contractorAgencyName,
    };
  }
}
