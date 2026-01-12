package com.example.myapplication;

import android.os.Bundle;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentActivity;
import androidx.viewpager2.adapter.FragmentStateAdapter;
import com.example.myapplication.fragments.PatientDossierFragment;
import com.example.myapplication.fragments.PatientHistoryFragment;
import com.example.myapplication.fragments.PatientNotesFragment;

public class PatientDossierPagerAdapter extends FragmentStateAdapter {

    private final String patientId;
    private final String patientName;

    public PatientDossierPagerAdapter(@NonNull FragmentActivity fragmentActivity, String patientId, String patientName) {
        super(fragmentActivity);
        this.patientId = patientId;
        this.patientName = patientName;
    }

    @NonNull
    @Override
    public Fragment createFragment(int position) {
        switch (position) {
            case 0:
                PatientDossierFragment infoFragment = new PatientDossierFragment();
                Bundle args = new Bundle();
                args.putString("patientId", patientId);
                args.putString("patientName", patientName);
                infoFragment.setArguments(args);
                return infoFragment;
            case 1:
                PatientHistoryFragment historyFragment = new PatientHistoryFragment();
                Bundle historyArgs = new Bundle();
                historyArgs.putString("patientId", patientId);
                historyArgs.putString("patientName", patientName);
                historyFragment.setArguments(historyArgs);
                return historyFragment;
            case 2:
                PatientNotesFragment notesFragment = new PatientNotesFragment();
                Bundle notesArgs = new Bundle();
                notesArgs.putString("patientId", patientId);
                notesArgs.putString("patientName", patientName);
                notesFragment.setArguments(notesArgs);
                return notesFragment;
            default:
                return new PatientDossierFragment();
        }
    }

    @Override
    public int getItemCount() {
        return 3;
    }
}
