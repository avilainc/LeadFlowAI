import React, { useState } from 'react';
import { TextField, Button, Typography, Box } from '@mui/material';
import api from '../services/api';

const IngestLead: React.FC = () => {
  const [form, setForm] = useState({ name: '', email: '', phone: '' });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.post('/leads/ingest', form);
      alert('Lead enviado com sucesso!');
    } catch (error) {
      console.error('Erro ao enviar lead:', error);
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ maxWidth: 400 }}>
      <Typography variant="h5" gutterBottom>
        Ingerir Lead
      </Typography>
      <TextField
        fullWidth
        label="Nome"
        name="name"
        value={form.name}
        onChange={handleChange}
        margin="normal"
        required
      />
      <TextField
        fullWidth
        label="Email"
        name="email"
        type="email"
        value={form.email}
        onChange={handleChange}
        margin="normal"
        required
      />
      <TextField
        fullWidth
        label="Telefone"
        name="phone"
        value={form.phone}
        onChange={handleChange}
        margin="normal"
      />
      <Button type="submit" variant="contained" sx={{ mt: 2 }}>
        Enviar
      </Button>
    </Box>
  );
};

export default IngestLead;