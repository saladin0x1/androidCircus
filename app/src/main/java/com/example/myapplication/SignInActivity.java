package com.example.myapplication;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;

import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.LoginRequest;
import com.example.myapplication.api.models.LoginResponse;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class SignInActivity extends AppCompatActivity {

    private EditText emailInput, passwordInput;
    private TextView signInButton;
    private TextView signUpLink;
    private SessionManager sessionManager;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_signin);

        sessionManager = new SessionManager(this);
        apiService = RetrofitClient.getApiService();

        emailInput = findViewById(R.id.emailInput);
        passwordInput = findViewById(R.id.passwordInput);
        signInButton = findViewById(R.id.signInButton);
        signUpLink = findViewById(R.id.signUpLink);

        signInButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String email = emailInput.getText().toString().trim();
                String password = passwordInput.getText().toString().trim();

                if (email.isEmpty() || password.isEmpty()) {
                    Toast.makeText(SignInActivity.this, "Veuillez remplir tous les champs", Toast.LENGTH_SHORT).show();
                    return;
                }

                performLogin(email, password);
            }
        });

        signUpLink.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(SignInActivity.this, SignUpActivity.class);
                startActivity(intent);
            }
        });
    }

    private void performLogin(String email, String password) {
        Toast.makeText(SignInActivity.this, "Connexion en cours...", Toast.LENGTH_SHORT).show();

        LoginRequest request = new LoginRequest(email, password);

        apiService.login(request).enqueue(new Callback<LoginResponse>() {
            @Override
            public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    LoginResponse loginResponse = response.body();
                    if (loginResponse.isSuccess()) {
                        LoginResponse.Data data = loginResponse.getData();
                        sessionManager.saveSession(
                            data.getToken(),
                            data.getUserId(),
                            data.getRole(),
                            data.getFirstName() + " " + data.getLastName(),
                            data.getEmail(),
                            data.getRoleSpecificId()
                        );

                        Toast.makeText(SignInActivity.this, "Connexion réussie", Toast.LENGTH_SHORT).show();
                        
                        Intent intent;
                        String role = data.getRole();
                        if ("Doctor".equalsIgnoreCase(role)) {
                            intent = new Intent(SignInActivity.this, DoctorHomeActivity.class);
                        } else if ("Clerk".equalsIgnoreCase(role)) {
                            intent = new Intent(SignInActivity.this, ClerkHomeActivity.class);
                        } else {
                            intent = new Intent(SignInActivity.this, HomeActivity.class);
                        }
                        
                        startActivity(intent);
                        finish();
                    } else {
                        Toast.makeText(SignInActivity.this, "Erreur: " + loginResponse.getError(), Toast.LENGTH_SHORT).show();
                    }
                } else {
                     Toast.makeText(SignInActivity.this, "Échec de la connexion", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<LoginResponse> call, Throwable t) {
                Toast.makeText(SignInActivity.this, "Erreur réseau: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }
}
