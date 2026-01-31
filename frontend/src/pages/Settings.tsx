import React, { useState } from 'react';
import { TextField, Button, Typography, Box, TextareaAutosize } from '@mui/material';
import api from '../services/api';

const Settings: React.FC = () => {
  const [faq, setFaq] = useState('');
  const [hours, setHours] = useState('');

  const handleSave = async () => {
    try {
      await api.post('/tenant/settings', { faq, businessHours: hours });
      alert('Configurações salvas!');
    } catch (error) {
      console.error('Erro ao salvar:', error);
    }
  };

  return (
    <Box sx={{ maxWidth: 600 }}>
      <Typography variant="h5" gutterBottom>
        Configurações do Tenant
      </Typography>
      <TextField
        fullWidth
        label="Horário de Funcionamento"
        value={hours}
        onChange={(e) => setHours(e.target.value)}
        margin="normal"
        placeholder="Ex: 9:00 - 18:00"
      />
      <Typography variant="body1" gutterBottom>
        FAQ
      </Typography>
      <TextareaAutosize
        minRows={5}
        style={{ width: '100%', padding: '8px' }}
        value={faq}
        onChange={(e) => setFaq(e.target.value)}
        placeholder="Perguntas frequentes..."
      />
      <Button variant="contained" onClick={handleSave} sx={{ mt: 2 }}>
        Salvar
      </Button>
    </Box>
  );
};

export default Settings;