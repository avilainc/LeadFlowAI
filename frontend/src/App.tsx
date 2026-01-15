import { Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout'
import LeadsPage from './pages/LeadsPage'
import LeadDetailPage from './pages/LeadDetailPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Navigate to="/leads" replace />} />
        <Route path="leads" element={<LeadsPage />} />
        <Route path="leads/:id" element={<LeadDetailPage />} />
      </Route>
    </Routes>
  )
}

export default App
