package com.example.myapplication;

import android.graphics.Color;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.api.models.AppointmentDTO;
import java.util.List;

public class AppointmentsAdapter extends RecyclerView.Adapter<AppointmentsAdapter.ViewHolder> {

    private List<AppointmentDTO> appointments;
    private OnAppointmentClickListener listener;

    public interface OnAppointmentClickListener {
        void onAppointmentClick(AppointmentDTO appointment);
    }

    public AppointmentsAdapter(List<AppointmentDTO> appointments, OnAppointmentClickListener listener) {
        this.appointments = appointments;
        this.listener = listener;
    }
    
    // Constructor for backward compatibility (defaults to null listener)
    public AppointmentsAdapter(List<AppointmentDTO> appointments) {
        this(appointments, null);
    }

    public void setOnAppointmentClickListener(OnAppointmentClickListener listener) {
        this.listener = listener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_appointment, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        AppointmentDTO appointment = appointments.get(position);
        
        holder.doctorNameText.setText("Dr. " + appointment.getDoctorName());
        holder.specializationText.setText(appointment.getDoctorSpecialization());
        holder.dateText.setText(appointment.getAppointmentDate().replace("T", " ").substring(0, 16));
        holder.reasonText.setText("Motif: " + appointment.getReason());
        
        String status = appointment.getStatus();
        holder.statusBadge.setText(status);
        
        // Color coding status
        if ("Scheduled".equalsIgnoreCase(status)) {
            holder.statusBadge.setTextColor(Color.parseColor("#2E7D32"));
            holder.statusBadge.getBackground().setTint(Color.parseColor("#E8F5E9"));
        } else if ("Completed".equalsIgnoreCase(status)) {
            holder.statusBadge.setTextColor(Color.parseColor("#1976D2"));
            holder.statusBadge.getBackground().setTint(Color.parseColor("#E3F2FD"));
        } else {
            holder.statusBadge.setTextColor(Color.parseColor("#C62828"));
            holder.statusBadge.getBackground().setTint(Color.parseColor("#FFEBEE"));
        }
        
        holder.itemView.setOnClickListener(v -> {
            if (listener != null) {
                listener.onAppointmentClick(appointment);
            }
        });
    }

    @Override
    public int getItemCount() {
        return appointments.size();
    }

    public void updateData(List<AppointmentDTO> newAppointments) {
        this.appointments = newAppointments;
        notifyDataSetChanged();
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView doctorNameText, specializationText, dateText, reasonText, statusBadge;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            doctorNameText = itemView.findViewById(R.id.doctorNameText);
            specializationText = itemView.findViewById(R.id.specializationText);
            dateText = itemView.findViewById(R.id.dateText);
            reasonText = itemView.findViewById(R.id.reasonText);
            statusBadge = itemView.findViewById(R.id.statusBadge);
        }
    }
}
