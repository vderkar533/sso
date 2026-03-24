import 'package:flutter/material.dart';

import '../controllers/auth_controller.dart';
import '../models/auth_models.dart';
import '../models/lookup_models.dart';
import '../services/registration_draft_storage.dart';
import '../theme/app_theme.dart';
import '../widgets/app_widgets.dart';
import 'registration_success_screen.dart';

class RegisterScreen extends StatefulWidget {
  final AuthController controller;

  const RegisterScreen({super.key, required this.controller});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _employeeIdController = TextEditingController();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _mobileController = TextEditingController();
  final _aadharController = TextEditingController();
  final _contractorAgencyController = TextEditingController();

  int? _userType;
  int? _selectedDepartmentId;
  int? _selectedGradeId;
  bool _lookupsLoaded = false;
  String? _validationMessage;
  bool _emailVerified = false;
  bool _showPassword = false;

  @override
  void initState() {
    super.initState();
    _emailController.addListener(_handleIdentityChange);
    _employeeIdController.addListener(_handleIdentityChange);
    _loadDraft();
    _loadLookups();
  }

  @override
  void dispose() {
    _emailController.removeListener(_handleIdentityChange);
    _employeeIdController.removeListener(_handleIdentityChange);
    _employeeIdController.dispose();
    _nameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _mobileController.dispose();
    _aadharController.dispose();
    _contractorAgencyController.dispose();
    super.dispose();
  }

  Future<void> _handleRegister() async {
    if (!_emailVerified) {
      setState(() {
        _validationMessage = 'Please verify your email before registering.';
      });
      return;
    }
    _userType ??= 1;
    if (_selectedDepartmentId == null || _selectedGradeId == null) {
      setState(() {
        _validationMessage = 'Please select department and grade.';
      });
      return;
    }
    if (_validationMessage != null) {
      setState(() {
        _validationMessage = null;
      });
    }
    final departmentId = _selectedDepartmentId ?? 0;
    final gradeId = _selectedGradeId ?? 0;
    final request = RegisterRequest(
      employeeId: _employeeIdController.text.trim(),
      name: _nameController.text.trim(),
      email: _emailController.text.trim(),
      password: _passwordController.text,
      departmentId: departmentId,
      gradeId: gradeId,
      mobileNumber: _mobileController.text.trim(),
      aadharNumber: _aadharController.text.trim(),
      userType: _userType ?? 1,
      contractorAgencyName: null,
    );

    await widget.controller.register(request);
    if (!mounted) return;
    if (widget.controller.errorMessage == null) {
      await RegistrationDraftStorage.clear();
      if (!mounted) return;
      await showCenterMessage(
        context,
        'Registration successful.',
        title: 'Success',
      );
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const RegistrationSuccessScreen()),
      );
    }
  }

  Future<void> _handleSendVerification() async {
    if (_employeeIdController.text.trim().isEmpty || _emailController.text.trim().isEmpty) {
      setState(() {
        _validationMessage = 'Employee ID and email are required for verification.';
      });
      return;
    }
    await _saveDraft(isVerified: false);
    await widget.controller.sendEmailVerification(
      _employeeIdController.text,
      _emailController.text,
    );
    if (!mounted) return;
    if (widget.controller.errorMessage == null) {
      setState(() {
        _emailVerified = false;
      });
      await showCenterMessage(
        context,
        'Verification email sent. Open it to verify.',
        title: 'Email verification',
      );
    }
  }

  Future<void> _handleRefreshVerification() async {
    if (_emailController.text.trim().isEmpty) {
      setState(() {
        _validationMessage = 'Email is required to check verification status.';
      });
      return;
    }

    final verified = await widget.controller.checkEmailVerified(_emailController.text);
    if (!mounted) return;

    if (verified) {
      setState(() {
        _emailVerified = true;
        _validationMessage = null;
      });
      await _saveDraft(isVerified: true);
      if (!mounted) return;
      await showCenterMessage(
        context,
        'Email verified. You can register now.',
        title: 'Email verified',
      );
    } else if (widget.controller.errorMessage != null) {
      setState(() {
        _validationMessage = widget.controller.errorMessage;
      });
    } else {
      setState(() {
        _validationMessage = 'Email not verified yet. Please click the email link.';
      });
    }
  }
  Future<void> _loadDraft() async {
    final draft = await RegistrationDraftStorage.load();
    if (draft == null) return;

    _employeeIdController.text = draft.employeeId;
    _nameController.text = draft.name;
    _emailController.text = draft.email;
    _passwordController.text = draft.password;
    _mobileController.text = draft.mobileNumber;
    _aadharController.text = draft.aadharNumber;
    _contractorAgencyController.text = draft.contractorAgencyName;
    _selectedDepartmentId = draft.departmentId;
    _selectedGradeId = draft.gradeId;
    _userType = draft.userType == 2 ? 1 : draft.userType;
    setState(() {
      _emailVerified = draft.emailVerified;
    });
  }

  Future<void> _saveDraft({required bool isVerified}) async {
    final draft = RegistrationDraft(
      employeeId: _employeeIdController.text.trim(),
      name: _nameController.text.trim(),
      email: _emailController.text.trim(),
      password: _passwordController.text,
      departmentId: _selectedDepartmentId,
      gradeId: _selectedGradeId,
      mobileNumber: _mobileController.text.trim(),
      aadharNumber: _aadharController.text.trim(),
      userType: _userType ?? 1,
      contractorAgencyName: '',
      emailVerified: isVerified,
    );
    await RegistrationDraftStorage.save(draft);
  }

  void _handleIdentityChange() {
    if (_emailVerified) {
      setState(() {
        _emailVerified = false;
      });
    }
    _saveDraft(isVerified: _emailVerified);
  }

  Future<void> _loadLookups() async {
    await widget.controller.loadLookups();
    if (!mounted) return;
    setState(() {
      _lookupsLoaded = true;
    });
  }

  List<DropdownMenuItem<int>> _buildDepartmentItems(List<Department> departments) {
    return departments
        .map(
          (dept) => DropdownMenuItem(
            value: dept.id,
            child: Text(
              dept.name,
              style: const TextStyle(color: AppTheme.brandBlack),
            ),
          ),
        )
        .toList();
  }

  List<DropdownMenuItem<int>> _buildGradeItems(List<Grade> grades) {
    return grades
        .map(
          (grade) => DropdownMenuItem(
            value: grade.id,
            child: Text(
              grade.displayName,
              style: const TextStyle(color: AppTheme.brandBlack),
            ),
          ),
        )
        .toList();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: widget.controller,
      builder: (context, _) {
        final isBusy = widget.controller.isBusy;
        final departments = widget.controller.departments;
        final grades = widget.controller.grades;
        return Scaffold(
          appBar: AppBar(title: const Text('Create account')),
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
                              title: 'Create your SSO account',
                              subtitle: 'Use your official @lloyds.in email.',
                            ),
                            if (_validationMessage != null) ErrorBanner(_validationMessage!),
                            if (widget.controller.errorMessage != null)
                              ErrorBanner(widget.controller.errorMessage!),
                            const SectionTitle('Account details'),
                            ResponsiveTwoColumn(
                              gap: 12,
                              left: AppTextField(controller: _employeeIdController, label: 'Employee ID'),
                              right: AppTextField(controller: _nameController, label: 'Full name'),
                            ),
                            const SizedBox(height: 12),
                            ResponsiveTwoColumn(
                              gap: 12,
                              left: AppTextField(
                                controller: _emailController,
                                label: 'Email (@lloyds.in)',
                                keyboardType: TextInputType.emailAddress,
                              ),
                              right: AppTextField(
                                controller: _passwordController,
                                label: 'Password',
                                obscureText: !_showPassword,
                                suffixIcon: IconButton(
                                  onPressed: () => setState(() => _showPassword = !_showPassword),
                                  icon: Icon(_showPassword ? Icons.visibility_off : Icons.visibility),
                                ),
                              ),
                            ),
                            const SectionTitle('Email verification'),
                            Text(
                              'Send a verification link to your work email, then open it to continue.',
                              style: Theme.of(context)
                                  .textTheme
                                  .bodySmall
                                  ?.copyWith(color: Colors.black54),
                            ),
                            const SizedBox(height: 8),
                            PrimaryButton(
                              label: isBusy ? 'Sending...' : 'Send verification email',
                              onPressed: isBusy ? null : _handleSendVerification,
                            ),
                            const SizedBox(height: 12),
                            TextButton(
                              onPressed: isBusy ? null : _handleRefreshVerification,
                              child: const Text('I already verified (check status)'),
                            ),
                            const SizedBox(height: 8),
                            Text(
                              _emailVerified ? 'Email verified.' : 'Email not verified yet.',
                              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                                    color: _emailVerified ? Colors.green.shade700 : Colors.red.shade700,
                                    fontWeight: FontWeight.w600,
                                  ),
                            ),
                            const SectionTitle('Organization'),
                            if (!_lookupsLoaded && isBusy)
                              const Padding(
                                padding: EdgeInsets.symmetric(vertical: 12),
                                child: Center(child: CircularProgressIndicator()),
                              ),
                            ResponsiveTwoColumn(
                              gap: 12,
                              left: AppDropdownField<int>(
                                initialValue: _selectedDepartmentId,
                                items: _buildDepartmentItems(departments),
                                onChanged: isBusy || departments.isEmpty
                                    ? null
                                    : (value) => setState(() => _selectedDepartmentId = value),
                                label: 'Department',
                                hint: departments.isEmpty ? 'Departments not loaded' : 'Select department',
                              ),
                              right: AppDropdownField<int>(
                                initialValue: _selectedGradeId,
                                items: _buildGradeItems(grades),
                                onChanged: isBusy || grades.isEmpty
                                    ? null
                                    : (value) => setState(() => _selectedGradeId = value),
                                label: 'Grade',
                                hint: grades.isEmpty ? 'Grades not loaded' : 'Select grade',
                              ),
                            ),
                            const SectionTitle('Contact'),
                            ResponsiveTwoColumn(
                              gap: 12,
                              left: AppTextField(
                                controller: _mobileController,
                                label: 'Mobile number',
                                keyboardType: TextInputType.phone,
                                maxLength: 10,
                              ),
                              right: AppTextField(
                                controller: _aadharController,
                                label: 'Aadhar number',
                                keyboardType: TextInputType.number,
                                maxLength: 12,
                              ),
                            ),
                            const SectionTitle('User type'),
                            AppDropdownField<int>(
                              initialValue: _userType ?? 1,
                              items: const [
                                DropdownMenuItem(
                                  value: 1,
                                  child: Text(
                                    'Employee',
                                    style: TextStyle(color: AppTheme.brandBlack),
                                  ),
                                ),
                              ],
                              onChanged: isBusy
                                  ? null
                                  : (value) {
                                      if (value == null) return;
                                      setState(() => _userType = value);
                                    },
                              label: 'User type',
                              hint: 'Select user type',
                            ),
                            const SizedBox(height: 16),
                            PrimaryButton(
                              label: isBusy ? 'Creating account...' : 'Register',
                              onPressed: isBusy || !_emailVerified ? null : _handleRegister,
                            ),
                            if (!_emailVerified) ...[
                              const SizedBox(height: 8),
                              const Text(
                                'Please verify your email before registering.',
                                style: TextStyle(color: Colors.red),
                              ),
                            ],
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
