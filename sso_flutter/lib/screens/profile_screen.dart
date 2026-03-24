import 'package:flutter/material.dart';

import '../config/app_config.dart';
import '../controllers/auth_controller.dart';
import '../models/auth_models.dart';
import '../models/lookup_models.dart';
import '../theme/app_theme.dart';
import '../widgets/app_widgets.dart';

class ProfileScreen extends StatefulWidget {
  final AuthController controller;

  const ProfileScreen({super.key, required this.controller});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _employeeIdController = TextEditingController();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _mobileController = TextEditingController();
  final _aadharController = TextEditingController();
  final _contractorAgencyController = TextEditingController();
  final _hierarchyController = TextEditingController();

  final _currentPasswordController = TextEditingController();
  final _newPasswordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();

  int? _selectedDepartmentId;
  int? _selectedGradeId;
  int? _selectedUserType;

  String? _seededUserId;
  String? _profileError;
  String? _passwordError;

  @override
  void initState() {
    super.initState();
    widget.controller.loadLookups();
  }

  @override
  void dispose() {
    _employeeIdController.dispose();
    _nameController.dispose();
    _emailController.dispose();
    _mobileController.dispose();
    _aadharController.dispose();
    _contractorAgencyController.dispose();
    _hierarchyController.dispose();
    _currentPasswordController.dispose();
    _newPasswordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  void _seedControllers(UserProfile profile) {
    if (_seededUserId == profile.userId) return;
    _seededUserId = profile.userId;
    _employeeIdController.text = profile.employeeId;
    _nameController.text = profile.name;
    _emailController.text = profile.email;
    _mobileController.text = profile.mobileNumber;
    _aadharController.text = profile.aadharNumber;
    _hierarchyController.text = profile.hierarchyLevel.toString();
    _selectedDepartmentId = profile.departmentId;
    _selectedGradeId = profile.gradeId;
    _selectedUserType = profile.userTypeId == 0 ? 1 : profile.userTypeId;
    if (_selectedUserType == 2) {
      _selectedUserType = 1;
    }
    _contractorAgencyController.text = profile.contractorAgencyName;
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

  Future<void> _handleUpdateProfile() async {
    setState(() {
      _profileError = null;
    });

    _selectedUserType ??= 1;
    if (_selectedDepartmentId == null || _selectedGradeId == null) {
      setState(() {
        _profileError = 'Please select department and grade.';
      });
      return;
    }

    if (_mobileController.text.trim().isEmpty || _aadharController.text.trim().isEmpty) {
      setState(() {
        _profileError = 'Mobile number and Aadhar number are required.';
      });
      return;
    }

    final request = UpdateProfileRequest(
      employeeId: _employeeIdController.text.trim(),
      name: _nameController.text.trim(),
      departmentId: _selectedDepartmentId!,
      gradeId: _selectedGradeId!,
      mobileNumber: _mobileController.text.trim(),
      aadharNumber: _aadharController.text.trim(),
      userType: _selectedUserType!,
      contractorAgencyName: null,
    );

    final success = await widget.controller.updateProfile(request);
    if (!mounted) return;

    if (success) {
      await showCenterMessage(
        context,
        'Profile updated successfully.',
        title: 'Profile update',
      );
    } else if (widget.controller.errorMessage != null) {
      setState(() {
        _profileError = widget.controller.errorMessage;
      });
    }
  }

  Future<void> _handleChangePassword() async {
    setState(() {
      _passwordError = null;
    });

    final email = _emailController.text.trim();
    if (email.isEmpty) {
      setState(() {
        _passwordError = 'Email is missing. Please refresh your profile.';
      });
      return;
    }

    if (_newPasswordController.text != _confirmPasswordController.text) {
      setState(() {
        _passwordError = 'New password and confirm password do not match.';
      });
      return;
    }

    final success = await widget.controller.changePassword(
      email,
      _currentPasswordController.text,
      _newPasswordController.text,
    );

    if (!mounted) return;

    if (success) {
      _currentPasswordController.clear();
      _newPasswordController.clear();
      _confirmPasswordController.clear();
      await showCenterMessage(
        context,
        'Password changed successfully.',
        title: 'Password updated',
      );
    } else if (widget.controller.errorMessage != null) {
      setState(() {
        _passwordError = widget.controller.errorMessage;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: widget.controller,
      builder: (context, _) {
        final profile = widget.controller.profile;
        if (profile != null) {
          _seedControllers(profile);
        }
        return DefaultTabController(
          length: 2,
          child: Scaffold(
            appBar: AppBar(
              title: const Text(
                AppConfig.appName,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
              toolbarHeight: 72,
              actions: [
                IconButton(
                  onPressed: widget.controller.isBusy ? null : widget.controller.refreshProfile,
                  icon: const Icon(Icons.refresh),
                ),
                IconButton(
                  onPressed: widget.controller.isBusy ? null : widget.controller.logout,
                  icon: const Icon(Icons.logout),
                ),
              ],
              bottom: const TabBar(
                labelColor: AppTheme.brandBlack,
                indicatorColor: AppTheme.brandRed,
                tabs: [
                  Tab(text: 'Edit details'),
                  Tab(text: 'Change password'),
                ],
              ),
            ),
            body: AppBackground(
              child: SafeArea(
                child: TabBarView(
                  children: [
                    _buildEditDetailsTab(profile),
                    _buildChangePasswordTab(),
                  ],
                ),
              ),
            ),
          ),
        );
      },
    );
  }

  Widget _buildEditDetailsTab(UserProfile? profile) {
    final departments = widget.controller.departments;
    final grades = widget.controller.grades;
    final canEdit = departments.isNotEmpty && grades.isNotEmpty;

    return SingleChildScrollView(
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
                    title: 'Your profile',
                    subtitle: 'Update your registration details.',
                  ),
                  if (_profileError != null) ErrorBanner(_profileError!),
                  if (widget.controller.errorMessage != null)
                    ErrorBanner(widget.controller.errorMessage!),
                  if (widget.controller.isBusy) ...[
                    const Center(child: CircularProgressIndicator()),
                  ] else if (profile == null) ...[
                    const Text('No profile data available.'),
                  ] else ...[
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
                        label: 'Email',
                        keyboardType: TextInputType.emailAddress,
                        readOnly: true,
                      ),
                      right: AppTextField(
                        controller: _mobileController,
                        label: 'Mobile number',
                        keyboardType: TextInputType.phone,
                      ),
                    ),
                    const SizedBox(height: 12),
                    if (canEdit)
                      ResponsiveTwoColumn(
                        gap: 12,
                        left: AppDropdownField<int>(
                          initialValue: _selectedDepartmentId,
                          items: _buildDepartmentItems(departments),
                          onChanged: (value) => setState(() => _selectedDepartmentId = value),
                          label: 'Department',
                          hint: 'Select department',
                        ),
                        right: AppDropdownField<int>(
                          initialValue: _selectedGradeId,
                          items: _buildGradeItems(grades),
                          onChanged: (value) => setState(() => _selectedGradeId = value),
                          label: 'Grade',
                          hint: 'Select grade',
                        ),
                      )
                    else
                      const Text('Departments not available. Please refresh.'),
                    const SizedBox(height: 12),
                    ResponsiveTwoColumn(
                      gap: 12,
                      left: AppTextField(
                        controller: _aadharController,
                        label: 'Aadhar number',
                        keyboardType: TextInputType.number,
                      ),
                      right: AppDropdownField<int>(
                        initialValue: _selectedUserType ?? 1,
                        items: const [
                          DropdownMenuItem(
                            value: 1,
                            child: Text(
                              'Employee',
                              style: TextStyle(color: AppTheme.brandBlack),
                            ),
                          ),
                        ],
                        onChanged: (value) => setState(() => _selectedUserType = value),
                        label: 'User type',
                        hint: 'Select user type',
                      ),
                    ),
                    const SizedBox(height: 12),
                    AppTextField(
                      controller: _hierarchyController,
                      label: 'Hierarchy level',
                      readOnly: true,
                    ),
                    const SizedBox(height: 16),
                    PrimaryButton(
                      label: widget.controller.isBusy ? 'Updating...' : 'Update details',
                      onPressed: widget.controller.isBusy || !canEdit ? null : _handleUpdateProfile,
                    ),
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildChangePasswordTab() {
    return SingleChildScrollView(
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
                    title: 'Change password',
                    subtitle: 'Set a new password for your account.',
                  ),
                  if (_passwordError != null) ErrorBanner(_passwordError!),
                  if (widget.controller.errorMessage != null)
                    ErrorBanner(widget.controller.errorMessage!),
                  AppTextField(
                    controller: _currentPasswordController,
                    label: 'Current password',
                    obscureText: true,
                  ),
                  const SizedBox(height: 12),
                  ResponsiveTwoColumn(
                    gap: 12,
                    left: AppTextField(
                      controller: _newPasswordController,
                      label: 'New password',
                      obscureText: true,
                    ),
                    right: AppTextField(
                      controller: _confirmPasswordController,
                      label: 'Confirm new password',
                      obscureText: true,
                    ),
                  ),
                  const SizedBox(height: 16),
                  PrimaryButton(
                    label: widget.controller.isBusy ? 'Updating...' : 'Change password',
                    onPressed: widget.controller.isBusy ? null : _handleChangePassword,
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class InfoTile extends StatelessWidget {
  final String label;
  final String value;

  const InfoTile({super.key, required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.blueGrey.shade50.withValues(alpha: 0.6),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: Colors.blueGrey.shade100),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label,
            style: Theme.of(context).textTheme.labelMedium?.copyWith(color: Colors.blueGrey.shade700),
          ),
          const SizedBox(height: 4),
          Text(
            value,
            style: Theme.of(context).textTheme.bodyLarge?.copyWith(color: AppTheme.brandBlack),
          ),
        ],
      ),
    );
  }
}
