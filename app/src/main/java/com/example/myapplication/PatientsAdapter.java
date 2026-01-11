package com.example.myapplication;

import android.graphics.Color;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.fragments.PatientsFragment;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

public class PatientsAdapter extends RecyclerView.Adapter<PatientsAdapter.ViewHolder> {

    private List<PatientsFragment.PatientInfo> patients;
    private OnPatientClickListener listener;

    public interface OnPatientClickListener {
        void onPatientClick(PatientsFragment.PatientInfo patient);
    }

    public PatientsAdapter(List<PatientsFragment.PatientInfo> patients, OnPatientClickListener listener) {
        this.patients = patients;
        this.listener = listener;
    }

    public void updateData(List<PatientsFragment.PatientInfo> newPatients) {
        this.patients = newPatients;
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_patient, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        PatientsFragment.PatientInfo patient = patients.get(position);

        // Set avatar letter (first letter of name)
        String firstLetter = patient.getName().substring(0, 1).toUpperCase(Locale.ROOT);
        holder.patientAvatar.setText(firstLetter);

        // Set name
        holder.patientNameText.setText(patient.getName());

        // Set appointment count
        String countText = patient.getAppointmentCount() + " rendez-vous";
        holder.appointmentCountText.setText(countText);

        // Click listener
        holder.itemView.setOnClickListener(v -> {
            if (listener != null) {
                listener.onPatientClick(patient);
            }
        });
    }

    @Override
    public int getItemCount() {
        return patients.size();
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView patientAvatar;
        TextView patientNameText;
        TextView appointmentCountText;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            patientAvatar = itemView.findViewById(R.id.patientAvatar);
            patientNameText = itemView.findViewById(R.id.patientNameText);
            appointmentCountText = itemView.findViewById(R.id.appointmentCountText);
        }
    }
}
