import { Outlet, Link, useLocation } from 'react-router-dom'
import { LayoutDashboard, Users, Settings } from 'lucide-react'

export default function Layout() {
  const location = useLocation()

  const navigation = [
    { name: 'Leads', href: '/leads', icon: Users },
    { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
    { name: 'Configurações', href: '/settings', icon: Settings },
  ]

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Sidebar */}
      <div className="fixed inset-y-0 left-0 w-64 bg-white shadow-lg">
        <div className="flex flex-col h-full">
          <div className="flex items-center justify-center h-16 px-4 bg-blue-600">
            <h1 className="text-xl font-bold text-white">LeadFlowAI</h1>
          </div>
          <nav className="flex-1 px-4 py-6 space-y-2">
            {navigation.map((item) => {
              const isActive = location.pathname.startsWith(item.href)
              return (
                <Link
                  key={item.name}
                  to={item.href}
                  className={`flex items-center px-4 py-3 text-sm font-medium rounded-lg transition-colors ${
                    isActive
                      ? 'bg-blue-50 text-blue-600'
                      : 'text-gray-700 hover:bg-gray-100'
                  }`}
                >
                  <item.icon className="w-5 h-5 mr-3" />
                  {item.name}
                </Link>
              )
            })}
          </nav>
        </div>
      </div>

      {/* Main content */}
      <div className="pl-64">
        <div className="p-8">
          <Outlet />
        </div>
      </div>
    </div>
  )
}
