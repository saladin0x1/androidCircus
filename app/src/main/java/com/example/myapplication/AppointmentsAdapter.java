package com.example.myapplication;

import android.content.res.ColorStateList;
import android.graphics.Color;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.api.models.AppointmentDTO;
import java.util.List;

public class AppointmentsAdapter extends RecyclerView.Adapter<AppointmentsAdapter.ViewHolder> {

    private List<AppointmentDTO> appointments;
    private OnAppointmentClickListener listener;
    private OnCancelClickListener cancelListener;
    private boolean showCancelButton;

    public interface OnAppointmentClickListener {
        void onAppointmentClick(AppointmentDTO appointment);
    }

    public interface OnCancelClickListener {
        void onCancelClick(AppointmentDTO appointment);
    }

    public AppointmentsAdapter(List<AppointmentDTO> appointments, OnAppointmentClickListener listener) {
        this.appointments = appointments != null ? appointments : new java.util.ArrayList<>();
        this.listener = listener;
        this.showCancelButton = false;
    }

    public AppointmentsAdapter(List<AppointmentDTO> appointments, OnAppointmentClickListener listener, boolean showCancelButton) {
        this.appointments = appointments != null ? appointments : new java.util.ArrayList<>();
        this.listener = listener;
        this.showCancelButton = showCancelButton;
    }

    // Constructor for backward compatibility (defaults to null listener)
    public AppointmentsAdapter(List<AppointmentDTO> appointments) {
        this(appointments, null);
    }

    public void setOnAppointmentClickListener(OnAppointmentClickListener listener) {
        this.listener = listener;
    }

    public void setOnCancelClickListener(OnCancelClickListener listener) {
        this.cancelListener = listener;
    }

    public void setShowCancelButton(boolean show) {
        this.showCancelButton = show;
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_appointment, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        if (position >= appointments.size()) return;

        AppointmentDTO appointment = appointments.get(position);
        if (appointment == null) return;

        // Safe text setting with null checks
        String doctorName = appointment.getDoctorName();
        holder.doctorNameText.setText(doctorName != null ? "Dr. " + doctorName : "Dr. Inconnu");

        String spec = appointment.getDoctorSpecialization();
        holder.specializationText.setText(spec != null ? spec : "Non spécifié");

        String dateStr = appointment.getAppointmentDate();
        if (dateStr != null && dateStr.length() >= 16) {
            holder.dateText.setText(dateStr.replace("T", " ").substring(0, 16));
        } else {
            holder.dateText.setText("Date non disponible");
        }

        String reason = appointment.getReason();
        holder.reasonText.setText(reason != null ? "Motif: " + reason : "Motif: Non spécifié");

        String status = appointment.getStatus();
        holder.statusBadge.setText(status != null ? status : "Inconnu");

        // Status icon and color coding
        String statusSafe = status != null ? status : "";
        int textColor, bgColorTint;
        int iconRes;

        if ("Scheduled".equalsIgnoreCase(statusSafe)) {
            textColor = Color.parseColor("#2E7D32");
            bgColorTint = Color.parseColor("#E8F5E9");
            iconRes = R.drawable.ic_check_circle;
        } else if ("Completed".equalsIgnoreCase(statusSafe)) {
            textColor = Color.parseColor("#1976D2");
            bgColorTint = Color.parseColor("#E3F2FD");
            iconRes = R.drawable.ic_done_all;
        } else if ("Pending".equalsIgnoreCase(statusSafe)) {
            textColor = Color.parseColor("#F57C00");
            bgColorTint = Color.parseColor("#FFF3E0");
            iconRes = R.drawable.ic_schedule;
        } else {
            // Cancelled or any other status
            textColor = Color.parseColor("#C62828");
            bgColorTint = Color.parseColor("#FFEBEE");
            iconRes = R.drawable.ic_cancel;
        }

        holder.statusBadge.setTextColor(textColor);
        if (holder.statusBadge.getBackground() != null) {
            holder.statusBadge.getBackground().setTint(bgColorTint);
        }
        holder.statusIcon.setImageResource(iconRes);
        holder.statusIcon.setImageTintList(ColorStateList.valueOf(textColor));

        // Show cancel button only for scheduled appointments when enabled
        boolean isScheduled = "Scheduled".equalsIgnoreCase(statusSafe);
        holder.cancelButton.setVisibility((showCancelButton && isScheduled) ? View.VISIBLE : View.GONE);

        holder.cancelButton.setOnClickListener(v -> {
            if (cancelListener != null) {
                cancelListener.onCancelClick(appointment);
            }
        });

        holder.itemView.setOnClickListener(v -> {
            if (listener != null) {
                listener.onAppointmentClick(appointment);
            }
        });
    }

    @Override
    public int getItemCount() {
        return appointments != null ? appointments.size() : 0;
    }

    public void updateData(List<AppointmentDTO> newAppointments) {
        this.appointments = newAppointments != null ? newAppointments : new java.util.ArrayList<>();
        notifyDataSetChanged();
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView doctorNameText, specializationText, dateText, reasonText, statusBadge, cancelButton;
        ImageView statusIcon;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            doctorNameText = itemView.findViewById(R.id.doctorNameText);
            specializationText = itemView.findViewById(R.id.specializationText);
            dateText = itemView.findViewById(R.id.dateText);
            reasonText = itemView.findViewById(R.id.reasonText);
            statusBadge = itemView.findViewById(R.id.statusBadge);
            statusIcon = itemView.findViewById(R.id.statusIcon);
            cancelButton = itemView.findViewById(R.id.cancelButton);
        }
    }
}
