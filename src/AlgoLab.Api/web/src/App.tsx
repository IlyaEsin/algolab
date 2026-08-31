import { useEffect, useState } from 'react'
import { api } from './api/client'
import type { ProblemSummary } from './api/types'
import { ProblemList } from './components/ProblemList'
import { ProblemPage } from './components/ProblemPage'

export default function App() {
  const [problems, setProblems] = useState<ProblemSummary[]>([])
  const [selected, setSelected] = useState<string | null>(null)

  useEffect(() => {
    api.listProblems().then((list) => {
      setProblems(list)
      setSelected((current) => current ?? list[0]?.slug ?? null)
    })
  }, [])

  return (
    <div className="flex h-screen bg-white text-slate-900">
      <ProblemList problems={problems} selected={selected} onSelect={setSelected} />
      {selected ? <ProblemPage slug={selected} /> : <p className="p-6">Задач пока нет.</p>}
    </div>
  )
}
