# Android Client (Example)

This sample uses an explicit login call to the SSO server, stores tokens, and calls a protected API.

```kotlin
// Kotlin sample (use Retrofit or OkHttp)
data class LoginRequest(
    val email: String,
    val password: String,
    val clientId: String
)

data class AuthResponse(
    val accessToken: String,
    val refreshToken: String,
    val idToken: String,
    val expiresIn: Int,
    val tokenType: String
)

// Login
val request = LoginRequest(
    email = "user@company.com",
    password = "P@ssword123",
    clientId = "mobile-app"
)

val response = api.login(request) // POST https://sso.yourcompany.com/api/auth/login
val accessToken = response.accessToken
val refreshToken = response.refreshToken

// Store tokens securely (EncryptedSharedPreferences or Keystore).
tokenStore.save(accessToken, refreshToken)

// Call protected API
val profile = api.getProfile("Bearer $accessToken")
```
