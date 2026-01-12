package com.example.myapplication;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.myapplication.api.models.PendingUser;
import java.util.ArrayList;
import java.util.List;

public class PendingUsersAdapter extends RecyclerView.Adapter<PendingUsersAdapter.ViewHolder> {

    private List<PendingUser> pendingUsers;
    private OnApprovalActionListener listener;

    public interface OnApprovalActionListener {
        void onApprove(PendingUser user);
        void onReject(PendingUser user);
    }

    public PendingUsersAdapter(List<PendingUser> pendingUsers, OnApprovalActionListener listener) {
        this.pendingUsers = pendingUsers != null ? pendingUsers : new ArrayList<>();
        this.listener = listener;
    }

    public void updateData(List<PendingUser> newUsers) {
        this.pendingUsers = newUsers != null ? newUsers : new ArrayList<>();
        notifyDataSetChanged();
    }

    public void removeUser(PendingUser user) {
        int position = -1;
        for (int i = 0; i < pendingUsers.size(); i++) {
            if (pendingUsers.get(i).getId().equals(user.getId())) {
                position = i;
                break;
            }
        }
        if (position >= 0) {
            pendingUsers.remove(position);
            notifyItemRemoved(position);
        }
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_pending_user, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        PendingUser user = pendingUsers.get(position);

        holder.nameText.setText(user.getFullName());
        holder.emailText.setText(user.getEmail());

        // Format requested date
        String dateStr = user.getRequestedDate();
        if (dateStr != null && dateStr.length() >= 10) {
            holder.dateText.setText("Demandé le: " + dateStr.substring(0, 10));
        } else {
            holder.dateText.setText("Demandé récemment");
        }

        holder.approveButton.setOnClickListener(v -> {
            if (listener != null) {
                listener.onApprove(user);
            }
        });

        holder.rejectButton.setOnClickListener(v -> {
            if (listener != null) {
                listener.onReject(user);
            }
        });
    }

    @Override
    public int getItemCount() {
        return pendingUsers.size();
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView nameText, emailText, dateText, approveButton, rejectButton;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            nameText = itemView.findViewById(R.id.nameText);
            emailText = itemView.findViewById(R.id.emailText);
            dateText = itemView.findViewById(R.id.dateText);
            approveButton = itemView.findViewById(R.id.approveButton);
            rejectButton = itemView.findViewById(R.id.rejectButton);
        }
    }
}
