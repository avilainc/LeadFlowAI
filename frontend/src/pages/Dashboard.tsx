import React, { useEffect, useState } from 'react';
import { Card, CardContent, Typography } from '@mui/material';
import api from '../services/api';

const Dashboard: React.FC = () => {
  const [stats, setStats] = useState({ totalLeads: 0, qualifiedLeads: 0 });

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const response = await api.get('/dashboard/stats'); // Endpoint fictício
        setStats(response.data);
      } catch (error) {
        console.error('Erro ao buscar estatísticas:', error);
      }
    };
    fetchStats();
  }, []);

  return (
    <div>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '24px' }}>
        <Card>
          <CardContent>
            <Typography variant="h5">Total de Leads</Typography>
            <Typography variant="h3">{stats.totalLeads}</Typography>
          </CardContent>
        </Card>
        <Card>
          <CardContent>
            <Typography variant="h5">Leads Qualificados</Typography>
            <Typography variant="h3">{stats.qualifiedLeads}</Typography>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default Dashboard;