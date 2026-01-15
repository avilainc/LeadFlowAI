import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { leadsApi, Lead } from '../api'
import { Search, Filter, ExternalLink } from 'lucide-react'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'

export default function LeadsPage() {
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [page, setPage] = useState(1)

  const { data, isLoading } = useQuery({
    queryKey: ['leads', search, statusFilter, page],
    queryFn: () => leadsApi.search({
      query: search || undefined,
      status: statusFilter || undefined,
      page,
      pageSize: 20,
    }),
  })

  const getStatusBadge = (status: string) => {
    const colors = {
      Received: 'bg-gray-100 text-gray-800',
      Normalized: 'bg-blue-100 text-blue-800',
      Enriched: 'bg-purple-100 text-purple-800',
      Qualified: 'bg-green-100 text-green-800',
      Responded: 'bg-teal-100 text-teal-800',
      Handoff: 'bg-orange-100 text-orange-800',
      Closed: 'bg-gray-200 text-gray-600',
      Failed: 'bg-red-100 text-red-800',
    }
    return colors[status as keyof typeof colors] || 'bg-gray-100 text-gray-800'
  }

  const getScoreColor = (score?: number) => {
    if (!score) return 'text-gray-400'
    if (score >= 80) return 'text-green-600 font-bold'
    if (score >= 50) return 'text-yellow-600 font-semibold'
    return 'text-red-600'
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Leads</h1>
        <p className="mt-2 text-gray-600">Gerencie e qualifique seus leads</p>
      </div>

      {/* Filtros */}
      <div className="mb-6 flex gap-4">
        <div className="flex-1 relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
          <input
            type="text"
            placeholder="Buscar por nome, email, telefone..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">Todos os status</option>
          <option value="Received">Recebido</option>
          <option value="Qualified">Qualificado</option>
          <option value="Responded">Respondido</option>
          <option value="Handoff">Handoff</option>
          <option value="Closed">Fechado</option>
        </select>
      </div>

      {/* Tabela de Leads */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Lead
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Contato
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Score
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Intenção
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Data
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Ações
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {isLoading ? (
              <tr>
                <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                  Carregando...
                </td>
              </tr>
            ) : data?.leads.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                  Nenhum lead encontrado
                </td>
              </tr>
            ) : (
              data?.leads.map((lead) => (
                <tr key={lead.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4">
                    <div className="text-sm font-medium text-gray-900">{lead.name}</div>
                    {lead.company && (
                      <div className="text-sm text-gray-500">{lead.company}</div>
                    )}
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-sm text-gray-900">{lead.phone}</div>
                    {lead.email && (
                      <div className="text-sm text-gray-500">{lead.email}</div>
                    )}
                  </td>
                  <td className="px-6 py-4">
                    <span className={`text-lg ${getScoreColor(lead.leadScore)}`}>
                      {lead.leadScore ?? '-'}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className="text-sm text-gray-900">
                      {lead.intent || '-'}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getStatusBadge(lead.status)}`}>
                      {lead.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-500">
                    {format(new Date(lead.createdAt), 'dd/MM/yyyy HH:mm', { locale: ptBR })}
                  </td>
                  <td className="px-6 py-4 text-right text-sm font-medium">
                    <Link
                      to={`/leads/${lead.id}`}
                      className="text-blue-600 hover:text-blue-900 inline-flex items-center"
                    >
                      Ver detalhes
                      <ExternalLink className="ml-1 w-4 h-4" />
                    </Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        {/* Paginação */}
        {data && data.totalPages > 1 && (
          <div className="px-6 py-4 flex items-center justify-between border-t border-gray-200">
            <div className="text-sm text-gray-700">
              Mostrando <span className="font-medium">{(page - 1) * 20 + 1}</span> a{' '}
              <span className="font-medium">
                {Math.min(page * 20, data.total)}
              </span>{' '}
              de <span className="font-medium">{data.total}</span> resultados
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Anterior
              </button>
              <button
                onClick={() => setPage(p => Math.min(data.totalPages, p + 1))}
                disabled={page === data.totalPages}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Próxima
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
