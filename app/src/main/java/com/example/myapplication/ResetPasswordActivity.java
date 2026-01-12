package com.example.myapplication;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.api.models.ResetPasswordRequest;
import com.example.myapplication.utils.ErrorMessageHelper;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ResetPasswordActivity extends AppCompatActivity {

    private EditText emailInput, newPasswordInput, confirmPasswordInput;
    private TextView submitButton;
    private TextView backToLoginButton;
    private ProgressBar progressBar;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_reset_password);

        apiService = RetrofitClient.getApiService();

        emailInput = findViewById(R.id.emailInput);
        newPasswordInput = findViewById(R.id.newPasswordInput);
        confirmPasswordInput = findViewById(R.id.confirmPasswordInput);
        submitButton = findViewById(R.id.submitButton);
        backToLoginButton = findViewById(R.id.backToLoginButton);
        progressBar = findViewById(R.id.progressBar);

        // Pre-fill email from intent if coming from forgot password
        String email = getIntent().getStringExtra("email");
        if (email != null) {
            emailInput.setText(email);
        }

        submitButton.setOnClickListener(v -> submitResetPassword());
        backToLoginButton.setOnClickListener(v -> {
            Intent intent = new Intent(ResetPasswordActivity.this, SignInActivity.class);
            startActivity(intent);
            finish();
        });
    }

    private void submitResetPassword() {
        String email = emailInput.getText().toString().trim();
        String newPassword = newPasswordInput.getText().toString().trim();
        String confirmPassword = confirmPasswordInput.getText().toString().trim();

        if (email.isEmpty() || newPassword.isEmpty() || confirmPassword.isEmpty()) {
            Toast.makeText(this, "Veuillez remplir tous les champs", Toast.LENGTH_SHORT).show();
            return;
        }

        if (!newPassword.equals(confirmPassword)) {
            Toast.makeText(this, "Les mots de passe ne correspondent pas", Toast.LENGTH_SHORT).show();
            return;
        }

        if (newPassword.length() < 6) {
            Toast.makeText(this, "Le mot de passe doit contenir au moins 6 caractères", Toast.LENGTH_SHORT).show();
            return;
        }

        showLoading(true);

        ResetPasswordRequest request = new ResetPasswordRequest(email, newPassword, confirmPassword);
        apiService.resetPassword(request).enqueue(new Callback<ApiResponse<Object>>() {
            @Override
            public void onResponse(Call<ApiResponse<Object>> call, Response<ApiResponse<Object>> response) {
                showLoading(false);
                if (response.isSuccessful()) {
                    Toast.makeText(ResetPasswordActivity.this, "Mot de passe réinitialisé avec succès", Toast.LENGTH_LONG).show();
                    Intent intent = new Intent(ResetPasswordActivity.this, SignInActivity.class);
                    startActivity(intent);
                    finishAffinity();
                } else {
                    String errorMsg = ErrorMessageHelper.getErrorMessage(ResetPasswordActivity.this,
                        response.code(), "Erreur lors de la réinitialisation");
                    Toast.makeText(ResetPasswordActivity.this, errorMsg, Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Object>> call, Throwable t) {
                showLoading(false);
                Toast.makeText(ResetPasswordActivity.this, "Vérifiez votre connexion internet", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void showLoading(boolean show) {
        if (progressBar != null) {
            progressBar.setVisibility(show ? View.VISIBLE : View.GONE);
        }
        submitButton.setEnabled(!show);
        emailInput.setEnabled(!show);
        newPasswordInput.setEnabled(!show);
        confirmPasswordInput.setEnabled(!show);
    }
}
