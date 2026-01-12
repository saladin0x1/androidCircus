package com.example.myapplication;

import android.os.Bundle;
import android.view.View;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.api.models.PendingUser;
import com.example.myapplication.utils.ErrorMessageHelper;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ClerkApprovalsActivity extends AppCompatActivity implements PendingUsersAdapter.OnApprovalActionListener {

    private ImageView backButton;
    private TextView pendingCountText;
    private RecyclerView pendingUsersRecyclerView;
    private View emptyStateLayout;
    private ProgressBar progressBar;
    private PendingUsersAdapter adapter;
    private SessionManager sessionManager;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_clerk_approvals);

        sessionManager = new SessionManager(this);
        apiService = RetrofitClient.getApiService();

        backButton = findViewById(R.id.backButton);
        pendingCountText = findViewById(R.id.pendingCountText);
        pendingUsersRecyclerView = findViewById(R.id.pendingUsersRecyclerView);
        emptyStateLayout = findViewById(R.id.emptyStateLayout);
        progressBar = findViewById(R.id.progressBar);

        backButton.setOnClickListener(v -> finish());

        pendingUsersRecyclerView.setLayoutManager(new LinearLayoutManager(this));
        adapter = new PendingUsersAdapter(new ArrayList<>(), this);
        pendingUsersRecyclerView.setAdapter(adapter);

        loadPendingUsers();
    }

    private void loadPendingUsers() {
        showLoading(true);

        apiService.getPendingUsers().enqueue(new Callback<ApiResponse<List<PendingUser>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<PendingUser>>> call, Response<ApiResponse<List<PendingUser>>> response) {
                showLoading(false);
                if (response.isSuccessful() && response.body() != null && response.body().isSuccess()) {
                    List<PendingUser> users = response.body().getData();
                    adapter.updateData(users);
                    updateCount(users.size());
                    updateEmptyState(users.isEmpty());
                } else {
                    String errorMsg = ErrorMessageHelper.getErrorMessage(ClerkApprovalsActivity.this,
                        response.code(), "Erreur lors du chargement");
                    Toast.makeText(ClerkApprovalsActivity.this, errorMsg, Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<PendingUser>>> call, Throwable t) {
                showLoading(false);
                Toast.makeText(ClerkApprovalsActivity.this, "Vérifiez votre connexion internet", Toast.LENGTH_SHORT).show();
            }
        });
    }

    @Override
    public void onApprove(PendingUser user) {
        new AlertDialog.Builder(this)
                .setTitle("Approuver l'utilisateur")
                .setMessage("Voulez-vous approuver " + user.getFullName() + " ?")
                .setPositiveButton("Oui", (dialog, which) -> approveUser(user))
                .setNegativeButton("Non", null)
                .show();
    }

    @Override
    public void onReject(PendingUser user) {
        new AlertDialog.Builder(this)
                .setTitle("Rejeter l'utilisateur")
                .setMessage("Voulez-vous rejeter " + user.getFullName() + " ?")
                .setPositiveButton("Oui", (dialog, which) -> rejectUser(user))
                .setNegativeButton("Non", null)
                .show();
    }

    private void approveUser(PendingUser user) {
        apiService.approveUser(user.getId()).enqueue(new Callback<ApiResponse<Object>>() {
            @Override
            public void onResponse(Call<ApiResponse<Object>> call, Response<ApiResponse<Object>> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(ClerkApprovalsActivity.this, "Utilisateur approuvé", Toast.LENGTH_SHORT).show();
                    adapter.removeUser(user);
                    updateCount(adapter.getItemCount());
                    updateEmptyState(adapter.getItemCount() == 0);
                } else {
                    String errorMsg = ErrorMessageHelper.getErrorMessage(ClerkApprovalsActivity.this,
                        response.code(), "Erreur lors de l'approbation");
                    Toast.makeText(ClerkApprovalsActivity.this, errorMsg, Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Object>> call, Throwable t) {
                Toast.makeText(ClerkApprovalsActivity.this, "Vérifiez votre connexion internet", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void rejectUser(PendingUser user) {
        apiService.rejectUser(user.getId()).enqueue(new Callback<ApiResponse<Object>>() {
            @Override
            public void onResponse(Call<ApiResponse<Object>> call, Response<ApiResponse<Object>> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(ClerkApprovalsActivity.this, "Utilisateur rejeté", Toast.LENGTH_SHORT).show();
                    adapter.removeUser(user);
                    updateCount(adapter.getItemCount());
                    updateEmptyState(adapter.getItemCount() == 0);
                } else {
                    String errorMsg = ErrorMessageHelper.getErrorMessage(ClerkApprovalsActivity.this,
                        response.code(), "Erreur lors du rejet");
                    Toast.makeText(ClerkApprovalsActivity.this, errorMsg, Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<Object>> call, Throwable t) {
                Toast.makeText(ClerkApprovalsActivity.this, "Vérifiez votre connexion internet", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void updateCount(int count) {
        pendingCountText.setText(count + " en attente");
    }

    private void updateEmptyState(boolean isEmpty) {
        emptyStateLayout.setVisibility(isEmpty ? View.VISIBLE : View.GONE);
        pendingUsersRecyclerView.setVisibility(isEmpty ? View.GONE : View.VISIBLE);
    }

    private void showLoading(boolean show) {
        if (progressBar != null) {
            progressBar.setVisibility(show ? View.VISIBLE : View.GONE);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        loadPendingUsers();
    }
}
