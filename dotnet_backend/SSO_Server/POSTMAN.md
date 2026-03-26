# Postman Examples (No Swagger)

Base URL: `https://localhost:5001`

Headers used for protected calls:

```
Authorization: Bearer {access_token}
Content-Type: application/json
```

## 1) Send Email Verification

`POST /api/auth/send-email-verification`

```json
{
  "employeeId": "EMP123",
  "email": "user@company.com"
}
```

## 2) Verify Email

`GET /api/auth/verify-email?email=user@company.com&token={token}`

## 3) Register User

`POST /api/auth/register`

```json
{
  "employeeId": "EMP123",
  "name": "Jane Doe",
  "email": "user@company.com",
  "password": "P@ssword123",
  "departmentId": 1,
  "gradeId": 10,
  "mobileNumber": "9876543210",
  "aadharNumber": "123456789012",
  "userType": 1
}
```

## 4) Login

`POST /api/auth/login`

```json
{
  "email": "user@company.com",
  "password": "P@ssword123",
  "clientId": "internal-portal"
}
```

## 5) Forgot Password

`POST /api/auth/forgot-password`

```json
{
  "email": "user@company.com"
}
```

## 6) Reset Password

`POST /api/auth/reset-password`

```json
{
  "email": "user@company.com",
  "token": "{reset_token}",
  "newPassword": "NewP@ssword123"
}
```

## 7) Change Password

`POST /api/auth/change-password`

```json
{
  "email": "user@company.com",
  "currentPassword": "P@ssword123",
  "newPassword": "NewP@ssword123"
}
```

## 8) Refresh Token

`POST /api/auth/refresh-token`

```json
{
  "refreshToken": "{refresh_token}",
  "clientId": "internal-portal"
}
```

## 9) Logout

`POST /api/auth/logout`

```json
{
  "refreshToken": "{refresh_token}"
}
```

## 10) Call Protected API

`GET /api/user/profile`

## 11) Update Profile

`PUT /api/user/profile`

```json
{
  "employeeId": "EMP123",
  "name": "Jane Doe",
  "departmentId": 1,
  "gradeId": 10,
  "mobileNumber": "9876543210",
  "aadharNumber": "123456789012",
  "userType": 1,
  "contractorAgencyName": "Optional if contractor"
}
```

## 12) Get Employee Details By EmployeeId (Public)

`GET /api/user/employee/{employeeId}`

Example:

`GET /api/user/employee/EMP123`

## 13) List Departments

`GET /api/lookups/departments`

## 14) List Grades

`GET /api/lookups/grades`
