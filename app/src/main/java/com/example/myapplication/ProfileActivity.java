package com.example.myapplication;

import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.UpdateProfileRequest;
import com.example.myapplication.api.models.UserProfile;
import com.example.myapplication.repository.UserRepository;

public class ProfileActivity extends AppCompatActivity {

    private TextView emailText;
    private EditText nameInput, phoneInput;
    private TextView updateButton, backButton;
    private ProgressBar progressBar;
    private SessionManager sessionManager;
    private UserRepository userRepo;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_profile);

        sessionManager = new SessionManager(this);
        userRepo = UserRepository.getInstance(this);

        emailText = findViewById(R.id.emailText);
        nameInput = findViewById(R.id.nameInput);
        phoneInput = findViewById(R.id.phoneInput);
        updateButton = findViewById(R.id.updateButton);
        backButton = findViewById(R.id.backButton);
        progressBar = findViewById(R.id.progressBar);

        // Load user data from session
        emailText.setText(sessionManager.getUserEmail());
        nameInput.setText(sessionManager.getUserName());

        // Load full profile from API
        loadProfile();

        updateButton.setOnClickListener(v -> updateProfile());
        backButton.setOnClickListener(v -> finish());
    }

    private void loadProfile() {
        userRepo.getProfile(new UserRepository.RepositoryCallback<UserProfile>() {
            @Override
            public void onSuccess(UserProfile profile) {
                nameInput.setText(profile.getFirstName() + " " + profile.getLastName());
                phoneInput.setText(profile.getPhone() != null ? profile.getPhone() : "");
            }

            @Override
            public void onError(String message) {
                // Silently fail - session data is already loaded
            }
        });
    }

    private void updateProfile() {
        String fullName = nameInput.getText().toString().trim();
        String phone = phoneInput.getText().toString().trim();

        if (fullName.isEmpty()) {
            Toast.makeText(this, "Le nom ne peut pas être vide", Toast.LENGTH_SHORT).show();
            return;
        }

        showLoading(true);

        // Split name into first and last
        String firstName = fullName;
        String lastName = "";
        if (fullName.contains(" ")) {
            int splitIndex = fullName.indexOf(" ");
            firstName = fullName.substring(0, splitIndex);
            lastName = fullName.substring(splitIndex + 1);
        }

        UpdateProfileRequest request = new UpdateProfileRequest(firstName, lastName, phone);

        userRepo.updateProfile(request, new UserRepository.RepositoryCallback<UserProfile>() {
            @Override
            public void onSuccess(UserProfile profile) {
                showLoading(false);
                sessionManager.updateUserName(profile.getFirstName() + " " + profile.getLastName());
                Toast.makeText(ProfileActivity.this, "Profil mis à jour", Toast.LENGTH_SHORT).show();
                finish();
            }

            @Override
            public void onError(String message) {
                showLoading(false);
                Toast.makeText(ProfileActivity.this, message, Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void showLoading(boolean show) {
        if (progressBar != null) {
            progressBar.setVisibility(show ? View.VISIBLE : View.GONE);
        }
        updateButton.setEnabled(!show);
        nameInput.setEnabled(!show);
        phoneInput.setEnabled(!show);
    }
}
