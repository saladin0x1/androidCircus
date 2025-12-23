# Android API Integration Guide

## RETROFIT SETUP (30 mins)

### 1. Add dependencies to `app/build.gradle`:

```gradle
dependencies {
    // Existing dependencies...

    // Retrofit
    implementation 'com.squareup.retrofit2:retrofit:2.9.0'
    implementation 'com.squareup.retrofit2:converter-gson:2.9.0'

    // OkHttp
    implementation 'com.squareup.okhttp3:okhttp:4.11.0'
    implementation 'com.squareup.okhttp3:logging-interceptor:4.11.0'

    // Optional: ViewModels & LiveData (if not using)
    implementation 'androidx.lifecycle:lifecycle-viewmodel:2.6.2'
    implementation 'androidx.lifecycle:lifecycle-livedata:2.6.2'
}
```

### 2. Update `AndroidManifest.xml`:

```xml
<manifest>
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />

    <application
        android:usesCleartextTraffic="true"
        android:networkSecurityConfig="@xml/network_security_config">
        <!-- ... -->
    </application>
</manifest>
```

### 3. Create `res/xml/network_security_config.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
    <base-config cleartextTrafficPermitted="true">
        <trust-anchors>
            <certificates src="system" />
        </trust-anchors>
    </base-config>
</network-security-config>
```

---

## 4. Create API Models

### `app/src/main/java/com/example/androidcircus/api/models/`

#### LoginRequest.java
```java
package com.example.androidcircus.api.models;

public class LoginRequest {
    private String email;
    private String password;

    public LoginRequest(String email, String password) {
        this.email = email;
        this.password = password;
    }

    // Getters and setters
}
```

#### LoginResponse.java
```java
package com.example.androidcircus.api.models;

public class LoginResponse {
    private boolean success;
    private Data data;

    public static class Data {
        private String userId;
        private String firstName;
        private String lastName;
        private String email;
        private String role;
        private String token;
        private String refreshToken;

        // Getters and setters
    }

    // Getters and setters
}
```

#### AppointmentDTO.java
```java
package com.example.androidcircus.api.models;

public class AppointmentDTO {
    private String id;
    private String patientId;
    private String doctorId;
    private String appointmentDate;
    private String reason;
    private String status;
    private String notes;

    // Getters and setters
}
```

#### ApiResponse.java (generic wrapper)
```java
package com.example.androidcircus.api.models;

public class ApiResponse<T> {
    private boolean success;
    private T data;
    private Error error;

    public static class Error {
        private String code;
        private String message;

        // Getters and setters
    }

    // Getters and setters
}
```

---

## 5. Create API Service Interface

### `ApiService.java`

```java
package com.example.androidcircus.api;

import com.example.androidcircus.api.models.*;
import java.util.List;
import retrofit2.Call;
import retrofit2.http.*;

public interface ApiService {

    // Auth endpoints
    @POST("auth/login")
    Call<LoginResponse> login(@Body LoginRequest request);

    @POST("auth/register")
    Call<LoginResponse> register(@Body RegisterRequest request);

    @POST("auth/logout")
    Call<ApiResponse<Void>> logout(@Header("Authorization") String token);

    // Appointments
    @GET("appointments")
    Call<ApiResponse<List<AppointmentDTO>>> getAppointments(
        @Header("Authorization") String token,
        @Query("status") String status
    );

    @POST("appointments")
    Call<ApiResponse<AppointmentDTO>> createAppointment(
        @Header("Authorization") String token,
        @Body AppointmentDTO appointment
    );

    @PUT("appointments/{id}")
    Call<ApiResponse<AppointmentDTO>> updateAppointment(
        @Header("Authorization") String token,
        @Path("id") String appointmentId,
        @Body AppointmentDTO appointment
    );

    @DELETE("appointments/{id}")
    Call<ApiResponse<Void>> deleteAppointment(
        @Header("Authorization") String token,
        @Path("id") String appointmentId
    );

    // Doctors
    @GET("doctors")
    Call<ApiResponse<List<DoctorDTO>>> getDoctors();

    @GET("doctors/{id}")
    Call<ApiResponse<DoctorDTO>> getDoctor(@Path("id") String doctorId);

    // Patient specific
    @GET("patients/profile")
    Call<ApiResponse<PatientDTO>> getProfile(@Header("Authorization") String token);

    // Clerk endpoints
    @GET("clerk/dashboard")
    Call<ApiResponse<DashboardDTO>> getDashboard(@Header("Authorization") String token);
}
```

---

## 6. Create Retrofit Client

### `RetrofitClient.java`

```java
package com.example.androidcircus.api;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import java.util.concurrent.TimeUnit;

public class RetrofitClient {

    // CHANGE THIS TO YOUR VPS IP/DOMAIN
    private static final String BASE_URL = "http://YOUR_VPS_IP:5000/api/";

    private static Retrofit retrofit = null;

    public static Retrofit getClient() {
        if (retrofit == null) {

            // Logging interceptor
            HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
            logging.setLevel(HttpLoggingInterceptor.Level.BODY);

            // OkHttp client
            OkHttpClient client = new OkHttpClient.Builder()
                .addInterceptor(logging)
                .connectTimeout(30, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(30, TimeUnit.SECONDS)
                .build();

            retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(client)
                .addConverterFactory(GsonConverterFactory.create())
                .build();
        }
        return retrofit;
    }

    public static ApiService getApiService() {
        return getClient().create(ApiService.class);
    }
}
```

---

## 7. Create Session Manager (for storing token)

### `SessionManager.java`

```java
package com.example.androidcircus.utils;

import android.content.Context;
import android.content.SharedPreferences;

public class SessionManager {
    private static final String PREF_NAME = "ClinicSession";
    private static final String KEY_TOKEN = "token";
    private static final String KEY_USER_ID = "userId";
    private static final String KEY_ROLE = "role";
    private static final String KEY_NAME = "name";
    private static final String KEY_EMAIL = "email";

    private SharedPreferences prefs;
    private SharedPreferences.Editor editor;

    public SessionManager(Context context) {
        prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        editor = prefs.edit();
    }

    public void saveSession(String token, String userId, String role, String name, String email) {
        editor.putString(KEY_TOKEN, token);
        editor.putString(KEY_USER_ID, userId);
        editor.putString(KEY_ROLE, role);
        editor.putString(KEY_NAME, name);
        editor.putString(KEY_EMAIL, email);
        editor.apply();
    }

    public String getToken() {
        return prefs.getString(KEY_TOKEN, null);
    }

    public String getAuthHeader() {
        String token = getToken();
        return token != null ? "Bearer " + token : null;
    }

    public String getRole() {
        return prefs.getString(KEY_ROLE, null);
    }

    public boolean isLoggedIn() {
        return getToken() != null;
    }

    public void clearSession() {
        editor.clear();
        editor.apply();
    }
}
```

---

## 8. Update Login Activity

### `LoginActivity.java` (key changes)

```java
public class LoginActivity extends AppCompatActivity {

    private SessionManager sessionManager;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        sessionManager = new SessionManager(this);
        apiService = RetrofitClient.getApiService();

        // Check if already logged in
        if (sessionManager.isLoggedIn()) {
            navigateToHome();
            return;
        }

        Button loginButton = findViewById(R.id.btnLogin);
        loginButton.setOnClickListener(v -> handleLogin());
    }

    private void handleLogin() {
        String email = emailEditText.getText().toString();
        String password = passwordEditText.getText().toString();

        // Show loading
        progressBar.setVisibility(View.VISIBLE);

        LoginRequest request = new LoginRequest(email, password);

        apiService.login(request).enqueue(new Callback<LoginResponse>() {
            @Override
            public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                progressBar.setVisibility(View.GONE);

                if (response.isSuccessful() && response.body() != null) {
                    LoginResponse.Data data = response.body().getData();

                    // Save session
                    sessionManager.saveSession(
                        data.getToken(),
                        data.getUserId(),
                        data.getRole(),
                        data.getFirstName() + " " + data.getLastName(),
                        data.getEmail()
                    );

                    navigateToHome();
                } else {
                    Toast.makeText(LoginActivity.this,
                        "Login failed", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<LoginResponse> call, Throwable t) {
                progressBar.setVisibility(View.GONE);
                Toast.makeText(LoginActivity.this,
                    "Error: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void navigateToHome() {
        // Navigate based on role
        String role = sessionManager.getRole();
        Intent intent;
        switch (role) {
            case "Patient":
                intent = new Intent(this, PatientHomeActivity.class);
                break;
            case "Doctor":
                intent = new Intent(this, DoctorHomeActivity.class);
                break;
            case "Clerk":
                intent = new Intent(this, ClerkHomeActivity.class);
                break;
            default:
                return;
        }
        startActivity(intent);
        finish();
    }
}
```

---

## 9. Update Appointments Activity

### Replace SQLite with API calls:

```java
public class AppointmentsActivity extends AppCompatActivity {

    private SessionManager sessionManager;
    private ApiService apiService;
    private RecyclerView recyclerView;
    private AppointmentsAdapter adapter;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_appointments);

        sessionManager = new SessionManager(this);
        apiService = RetrofitClient.getApiService();

        recyclerView = findViewById(R.id.recyclerViewAppointments);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));

        loadAppointments();
    }

    private void loadAppointments() {
        String token = sessionManager.getAuthHeader();

        apiService.getAppointments(token, "all")
            .enqueue(new Callback<ApiResponse<List<AppointmentDTO>>>() {
                @Override
                public void onResponse(Call<ApiResponse<List<AppointmentDTO>>> call,
                                     Response<ApiResponse<List<AppointmentDTO>>> response) {
                    if (response.isSuccessful() && response.body() != null) {
                        List<AppointmentDTO> appointments = response.body().getData();
                        adapter = new AppointmentsAdapter(appointments);
                        recyclerView.setAdapter(adapter);
                    }
                }

                @Override
                public void onFailure(Call<ApiResponse<List<AppointmentDTO>>> call, Throwable t) {
                    Toast.makeText(AppointmentsActivity.this,
                        "Error loading appointments", Toast.LENGTH_SHORT).show();
                }
            });
    }
}
```

---

## 10. Migration Checklist

### Remove/Replace:
- [ ] Remove SQLite database helper class
- [ ] Remove local database file creation
- [ ] Replace all `db.query()` with API calls
- [ ] Replace all `db.insert()` with API POST calls
- [ ] Replace all `db.update()` with API PUT calls
- [ ] Replace all `db.delete()` with API DELETE calls

### Add:
- [ ] Retrofit dependencies
- [ ] API models (DTOs)
- [ ] ApiService interface
- [ ] RetrofitClient singleton
- [ ] SessionManager for token storage
- [ ] Error handling for network failures
- [ ] Loading indicators (ProgressBar)
- [ ] Offline handling (optional)

---

## TESTING SCRIPT

### Test on each phone:

#### Patient Phone:
```
1. Login with patient@clinic.com
2. View appointments
3. Create new appointment
4. Cancel appointment
```

#### Doctor Phone:
```
1. Login with doctor@clinic.com
2. View appointments
3. Complete an appointment
4. Add notes
```

#### Clerk Phone:
```
1. Login with clerk@clinic.com
2. View dashboard
3. Create patient
4. Create appointment
5. View all appointments
```

---

## TROUBLESHOOTING

### "Unable to resolve host" error:
- Check BASE_URL in RetrofitClient
- Verify VPS IP is correct
- Test with curl: `curl http://vps-ip:5000/api/health`

### "Cleartext HTTP traffic not permitted":
- Add `android:usesCleartextTraffic="true"` to manifest
- Add network_security_config.xml

### 401 Unauthorized:
- Check token is being sent: `Authorization: Bearer <token>`
- Verify token hasn't expired
- Check SessionManager.getAuthHeader()

### Connection timeout:
- Increase timeout in RetrofitClient
- Check VPS firewall allows port 5000
- Verify API is running on VPS

---

## QUICK WIN: Test with Postman First

Before changing Android code:
1. Import API into Postman
2. Test all endpoints
3. Verify response format matches Android models
4. Then update Android app
