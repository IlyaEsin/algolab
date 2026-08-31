import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { ProblemDetail } from '../api/types'
import { useReveal } from '../state/useReveal'
import { SolutionCard } from './SolutionCard'
import { Statement } from './Statement'

export function ProblemPage({ slug }: { slug: string }) {
  const [detail, setDetail] = useState<ProblemDetail | null>(null)
  const [error, setError] = useState<string | null>(null)
  const { revealed, reveal } = useReveal(slug)

  useEffect(() => {
    let cancelled = false
    setDetail(null)
    setError(null)
    api
      .getProblem(slug)
      .then((d) => {
        if (!cancelled) {
          setDetail(d)
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : String(err))
        }
      })
    return () => {
      cancelled = true
    }
  }, [slug])

  if (error) {
    return <p className="p-6 text-sm text-red-600">Не удалось загрузить задачу: {error}</p>
  }

  if (!detail) {
    return <p className="p-6 text-sm text-slate-500">Загрузка…</p>
  }

  return (
    <main className="flex-1 overflow-y-auto p-6">
      <h2 className="mb-1 text-xl font-semibold">{detail.summary.title}</h2>
      <a className="text-xs text-blue-600" href={detail.source} target="_blank" rel="noreferrer">
        оригинал задачи
      </a>
      <div className="my-4">
        <Statement markdown={detail.readme} />
      </div>
      <h3 className="mb-2 mt-6 font-medium">Решения</h3>
      {detail.solutions.map((s) => (
        <SolutionCard key={s.id} slug={slug} solution={s} revealed={revealed} onReveal={reveal} />
      ))}
    </main>
  )
}
