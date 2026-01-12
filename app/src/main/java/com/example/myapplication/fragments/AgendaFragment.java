package com.example.myapplication.fragments;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CalendarView;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AlertDialog;
import androidx.cardview.widget.CardView;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.AppointmentsAdapter;
import com.example.myapplication.R;
import com.example.myapplication.api.ApiService;
import com.example.myapplication.api.RetrofitClient;
import com.example.myapplication.api.SessionManager;
import com.example.myapplication.api.models.ApiResponse;
import com.example.myapplication.api.models.AppointmentDTO;
import com.example.myapplication.api.models.CompleteAppointmentRequest;
import com.example.myapplication.utils.ErrorMessageHelper;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class AgendaFragment extends Fragment {

    private CalendarView calendarView;
    private TextView selectedDateText;
    private RecyclerView appointmentsRecyclerView;
    private View emptyStateLayout;
    private AppointmentsAdapter adapter;
    private SessionManager sessionManager;
    private ApiService apiService;
    private List<AppointmentDTO> allAppointments = new ArrayList<>();
    private Date selectedDate;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_agenda, container, false);

        sessionManager = new SessionManager(requireContext());
        apiService = RetrofitClient.getApiService();

        // Initialize views
        calendarView = view.findViewById(R.id.calendarView);
        selectedDateText = view.findViewById(R.id.selectedDateText);
        appointmentsRecyclerView = view.findViewById(R.id.appointmentsRecyclerView);
        emptyStateLayout = view.findViewById(R.id.emptyStateLayout);

        // Setup RecyclerView
        appointmentsRecyclerView.setLayoutManager(new LinearLayoutManager(requireContext()));
        adapter = new AppointmentsAdapter(new ArrayList<>(), this::onAppointmentClick);
        appointmentsRecyclerView.setAdapter(adapter);

        // Set current date
        selectedDate = Calendar.getInstance().getTime();
        updateSelectedDateText();

        // Calendar date change listener
        calendarView.setOnDateChangeListener((view1, year, month, dayOfMonth) -> {
            Calendar cal = Calendar.getInstance();
            cal.set(year, month, dayOfMonth);
            selectedDate = cal.getTime();
            updateSelectedDateText();
            filterAppointmentsByDate();
        });

        // Load appointments
        loadAppointments();

        return view;
    }

    private void updateSelectedDateText() {
        SimpleDateFormat sdf = new SimpleDateFormat("dd MMMM yyyy", Locale.FRENCH);
        String dateStr = sdf.format(selectedDate);
        selectedDateText.setText(dateStr);
    }

    private void loadAppointments() {
        apiService.getAppointments("all").enqueue(new Callback<ApiResponse<List<AppointmentDTO>>>() {
            @Override
            public void onResponse(Call<ApiResponse<List<AppointmentDTO>>> call, Response<ApiResponse<List<AppointmentDTO>>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    ApiResponse<List<AppointmentDTO>> apiResponse = response.body();
                    if (apiResponse.isSuccess()) {
                        allAppointments = apiResponse.getData();
                        filterAppointmentsByDate();
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<List<AppointmentDTO>>> call, Throwable t) {
                Toast.makeText(requireContext(), "Erreur réseau", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void filterAppointmentsByDate() {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault());
        String selectedDateStr = sdf.format(selectedDate);

        List<AppointmentDTO> filtered = new ArrayList<>();
        for (AppointmentDTO appointment : allAppointments) {
            String appointmentDate = appointment.getAppointmentDate().substring(0, 10); // Extract date part
            if (appointmentDate.equals(selectedDateStr)) {
                filtered.add(appointment);
            }
        }

        if (filtered.isEmpty()) {
            emptyStateLayout.setVisibility(View.VISIBLE);
            appointmentsRecyclerView.setVisibility(View.GONE);
        } else {
            emptyStateLayout.setVisibility(View.GONE);
            appointmentsRecyclerView.setVisibility(View.VISIBLE);
            adapter.updateData(filtered);
        }
    }

    private void onAppointmentClick(AppointmentDTO appointment) {
        // Handle appointment click - show details/options
        if ("Scheduled".equalsIgnoreCase(appointment.getStatus())) {
            showCompleteConfirmation(appointment);
        } else {
            Toast.makeText(requireContext(), "Rendez-vous: " + appointment.getReason(), Toast.LENGTH_SHORT).show();
        }
    }

    private void showCompleteConfirmation(AppointmentDTO appointment) {
        new AlertDialog.Builder(requireContext())
                .setTitle("Compléter le rendez-vous")
                .setMessage("Voulez-vous vraiment marquer ce rendez-vous comme terminé ?")
                .setPositiveButton("Oui", (dialog, which) -> completeAppointment(appointment))
                .setNegativeButton("Non", null)
                .show();
    }

    private void completeAppointment(AppointmentDTO appointment) {
        CompleteAppointmentRequest request = new CompleteAppointmentRequest("");
        apiService.completeAppointment(appointment.getId(), request).enqueue(new Callback<ApiResponse<AppointmentDTO>>() {
            @Override
            public void onResponse(Call<ApiResponse<AppointmentDTO>> call, Response<ApiResponse<AppointmentDTO>> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(requireContext(), "Rendez-vous terminé", Toast.LENGTH_SHORT).show();
                    loadAppointments();
                } else {
                    String errorMsg = ErrorMessageHelper.getErrorMessage(requireContext(), response.code(), "Erreur lors de la mise à jour");
                    Toast.makeText(requireContext(), errorMsg, Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<AppointmentDTO>> call, Throwable t) {
                Toast.makeText(requireContext(), "Erreur réseau", Toast.LENGTH_SHORT).show();
            }
        });
    }

    @Override
    public void onResume() {
        super.onResume();
        loadAppointments();
    }
}
