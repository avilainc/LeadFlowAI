import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

// Adicionar token JWT automaticamente
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export interface Lead {
  id: string
  tenantId: string
  name: string
  phone: string
  email?: string
  company?: string
  city?: string
  state?: string
  message: string
  source: string
  sourceUrl?: string
  status: string
  leadScore?: number
  intent?: string
  urgency?: string
  serviceMatch?: string[]
  riskFlags?: string[]
  hasResponded: boolean
  respondedAt?: string
  isHandedOff: boolean
  handedOffAt?: string
  createdAt: string
  updatedAt?: string
}

export interface LeadEvent {
  id: string
  leadId: string
  eventType: string
  fromStatus?: string
  toStatus?: string
  description?: string
  actor?: string
  createdAt: string
}

export interface SearchLeadsResponse {
  leads: Lead[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export const leadsApi = {
  search: async (params: {
    query?: string
    status?: string
    source?: string
    startDate?: string
    endDate?: string
    page?: number
    pageSize?: number
  }): Promise<SearchLeadsResponse> => {
    const response = await api.get('/leads/search', { params })
    return response.data
  },

  getById: async (id: string): Promise<Lead> => {
    const response = await api.get(`/leads/${id}`)
    return response.data
  },

  getEvents: async (id: string): Promise<LeadEvent[]> => {
    const response = await api.get(`/leads/${id}/events`)
    return response.data
  },

  handoff: async (id: string): Promise<void> => {
    await api.post(`/leads/${id}/handoff`)
  },
}

export default api
