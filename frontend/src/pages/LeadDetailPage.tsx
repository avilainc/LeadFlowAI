import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { leadsApi } from '../api'
import { ArrowLeft, Phone, Mail, MapPin, Calendar, MessageSquare } from 'lucide-react'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'

export default function LeadDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { data: lead, isLoading } = useQuery({
    queryKey: ['lead', id],
    queryFn: () => leadsApi.getById(id!),
    enabled: !!id,
  })

  const { data: events } = useQuery({
    queryKey: ['lead-events', id],
    queryFn: () => leadsApi.getEvents(id!),
    enabled: !!id,
  })

  const handoffMutation = useMutation({
    mutationFn: () => leadsApi.handoff(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lead', id] })
      alert('Lead assumido com sucesso!')
    },
  })

  if (isLoading) {
    return <div className="text-center py-12">Carregando...</div>
  }

  if (!lead) {
    return <div className="text-center py-12">Lead não encontrado</div>
  }

  return (
    <div>
      <button
        onClick={() => navigate('/leads')}
        className="mb-6 flex items-center text-gray-600 hover:text-gray-900"
      >
        <ArrowLeft className="w-4 h-4 mr-2" />
        Voltar para lista
      </button>

      <div className="grid grid-cols-3 gap-6">
        {/* Coluna principal */}
        <div className="col-span-2 space-y-6">
          {/* Informações do Lead */}
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-start justify-between mb-6">
              <div>
                <h1 className="text-2xl font-bold text-gray-900">{lead.name}</h1>
                {lead.company && (
                  <p className="text-gray-600 mt-1">{lead.company}</p>
                )}
              </div>
              {lead.leadScore && (
                <div className="text-right">
                  <div className="text-3xl font-bold text-blue-600">{lead.leadScore}</div>
                  <div className="text-sm text-gray-500">Score</div>
                </div>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4 mb-6">
              <div className="flex items-center text-gray-700">
                <Phone className="w-5 h-5 mr-2 text-gray-400" />
                {lead.phone}
              </div>
              {lead.email && (
                <div className="flex items-center text-gray-700">
                  <Mail className="w-5 h-5 mr-2 text-gray-400" />
                  {lead.email}
                </div>
              )}
              {lead.city && (
                <div className="flex items-center text-gray-700">
                  <MapPin className="w-5 h-5 mr-2 text-gray-400" />
                  {lead.city}{lead.state && `, ${lead.state}`}
                </div>
              )}
              <div className="flex items-center text-gray-700">
                <Calendar className="w-5 h-5 mr-2 text-gray-400" />
                {format(new Date(lead.createdAt), 'dd/MM/yyyy HH:mm', { locale: ptBR })}
              </div>
            </div>

            <div className="border-t pt-4">
              <h3 className="font-semibold text-gray-900 mb-2 flex items-center">
                <MessageSquare className="w-5 h-5 mr-2" />
                Mensagem
              </h3>
              <p className="text-gray-700 whitespace-pre-wrap">{lead.message}</p>
            </div>
          </div>

          {/* Timeline de Eventos */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Timeline</h2>
            <div className="space-y-4">
              {events?.map((event) => (
                <div key={event.id} className="flex">
                  <div className="flex flex-col items-center mr-4">
                    <div className="w-3 h-3 bg-blue-500 rounded-full"></div>
                    <div className="w-0.5 h-full bg-gray-300 mt-2"></div>
                  </div>
                  <div className="flex-1 pb-4">
                    <div className="flex items-center justify-between mb-1">
                      <span className="font-semibold text-gray-900">{event.eventType}</span>
                      <span className="text-sm text-gray-500">
                        {format(new Date(event.createdAt), 'dd/MM HH:mm', { locale: ptBR })}
                      </span>
                    </div>
                    {event.description && (
                      <p className="text-sm text-gray-600">{event.description}</p>
                    )}
                    {event.actor && (
                      <p className="text-xs text-gray-500 mt-1">Por: {event.actor}</p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Status e Ações */}
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="font-semibold text-gray-900 mb-4">Status</h3>
            <div className="space-y-3">
              <div>
                <span className="text-sm text-gray-500">Status Atual</span>
                <p className="font-medium text-gray-900">{lead.status}</p>
              </div>
              {lead.intent && (
                <div>
                  <span className="text-sm text-gray-500">Intenção</span>
                  <p className="font-medium text-gray-900">{lead.intent}</p>
                </div>
              )}
              {lead.urgency && (
                <div>
                  <span className="text-sm text-gray-500">Urgência</span>
                  <p className="font-medium text-gray-900">{lead.urgency}</p>
                </div>
              )}
            </div>

            {!lead.isHandedOff && (
              <button
                onClick={() => handoffMutation.mutate()}
                className="mt-6 w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                Assumir Conversa
              </button>
            )}
          </div>

          {/* Serviços Identificados */}
          {lead.serviceMatch && lead.serviceMatch.length > 0 && (
            <div className="bg-white rounded-lg shadow p-6">
              <h3 className="font-semibold text-gray-900 mb-3">Serviços</h3>
              <div className="flex flex-wrap gap-2">
                {lead.serviceMatch.map((service, i) => (
                  <span
                    key={i}
                    className="px-3 py-1 bg-blue-100 text-blue-800 text-sm rounded-full"
                  >
                    {service}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Risk Flags */}
          {lead.riskFlags && lead.riskFlags.length > 0 && (
            <div className="bg-white rounded-lg shadow p-6">
              <h3 className="font-semibold text-gray-900 mb-3">Alertas</h3>
              <div className="space-y-2">
                {lead.riskFlags.map((flag, i) => (
                  <div key={i} className="px-3 py-2 bg-red-50 text-red-800 text-sm rounded">
                    {flag}
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Origem */}
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="font-semibold text-gray-900 mb-3">Origem</h3>
            <div className="space-y-2 text-sm">
              <div>
                <span className="text-gray-500">Fonte:</span>
                <span className="ml-2 font-medium">{lead.source}</span>
              </div>
              {lead.sourceUrl && (
                <div>
                  <span className="text-gray-500">URL:</span>
                  <a
                    href={lead.sourceUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="ml-2 text-blue-600 hover:underline block truncate"
                  >
                    {lead.sourceUrl}
                  </a>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
