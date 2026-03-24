class Department {
  final int id;
  final String name;

  Department({required this.id, required this.name});

  factory Department.fromJson(Map<String, dynamic> json) {
    return Department(
      id: json['id'] is num ? (json['id'] as num).toInt() : 0,
      name: json['name']?.toString() ?? '',
    );
  }
}

class Grade {
  final int id;
  final String code;
  final String title;
  final int hierarchyLevel;

  Grade({
    required this.id,
    required this.code,
    required this.title,
    required this.hierarchyLevel,
  });

  factory Grade.fromJson(Map<String, dynamic> json) {
    return Grade(
      id: json['id'] is num ? (json['id'] as num).toInt() : 0,
      code: json['code']?.toString() ?? '',
      title: json['title']?.toString() ?? '',
      hierarchyLevel: json['hierarchyLevel'] is num ? (json['hierarchyLevel'] as num).toInt() : 0,
    );
  }

  String get displayName => code.isEmpty ? title : '$code - $title';
}
