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
import com.example.myapplication.api.models.ForgotPasswordRequest;
import com.example.myapplication.utils.ErrorMessageHelper;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ForgotPasswordActivity extends AppCompatActivity {

    private EditText emailInput;
    private TextView submitButton;
    private TextView backToLoginButton;
    private ProgressBar progressBar;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_forgot_password);

        apiService = RetrofitClient.getApiService();

        emailInput = findViewById(R.id.emailInput);
        submitButton = findViewById(R.id.submitButton);
        backToLoginButton = findViewById(R.id.backToLoginButton);
        progressBar = findViewById(R.id.progressBar);

        submitButton.setOnClickListener(v -> submitForgotPassword());
        backToLoginButton.setOnClickListener(v -> finish());
    }

    private void submitForgotPassword() {
        String email = emailInput.getText().toString().trim();

        if (email.isEmpty()) {
            Toast.makeText(this, "Veuillez entrer votre adresse email", Toast.LENGTH_SHORT).show();
            return;
        }

        showLoading(true);

        ForgotPasswordRequest request = new ForgotPasswordRequest(email);
        apiService.forgotPassword(request).enqueue(new Callback<ApiResponse<Object>>() {
            @Override
            public void onResponse(Call<ApiResponse<Object>> call, Response<ApiResponse<Object>> response) {
                showLoading(false);
                if (response.isSuccessful() && response.body() != null) {
                    Toast.makeText(ForgotPasswordActivity.this,
                        "Si un compte existe avec cet email, un lien de réinitialisation a été envoyé",
                        Toast.LENGTH_LONG).show();
                    finish();
                } else {
                    String errorMsg = ErrorMessageHelper.getErrorMessage(ForgotPasswordActivity.this,
                        response.code(), "Erreur lors de l'envoi");
                    Toast.makeText(ForgotPasswordActivity.this, errorMsg, Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Object>> call, Throwable t) {
                showLoading(false);
                Toast.makeText(ForgotPasswordActivity.this, "Vérifiez votre connexion internet", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void showLoading(boolean show) {
        if (progressBar != null) {
            progressBar.setVisibility(show ? View.VISIBLE : View.GONE);
        }
        submitButton.setEnabled(!show);
        emailInput.setEnabled(!show);
    }
}
