import { useRef, useState } from 'react'
import { api } from '../api/client'
import type { RunStatus, RunnerPayload, SolutionKind } from '../api/types'
import { markSolved } from '../state/useReveal'

export function statusLabel(status: RunStatus): string {
  switch (status) {
    case 'Passed':
      return 'Все кейсы пройдены'
    case 'Failed':
      return 'Есть непройденные кейсы'
    case 'NotImplemented':
      return 'Решение не реализовано'
    case 'Error':
      return 'Решение упало с ошибкой'
  }
}

interface Props {
  slug: string
  solutionId: string
  kind: SolutionKind
}

export function RunPanel({ slug, solutionId, kind }: Props) {
  const [payload, setPayload] = useState<RunnerPayload | null>(null)
  const [busy, setBusy] = useState(false)

  // `disabled={busy}` alone isn't enough: React re-renders (and disables the button)
  // asynchronously, so a real double-click can reach this handler twice before that
  // update lands. This ref check is synchronous and closes that gap, so a second
  // click while a run is in flight is a no-op rather than a concurrent run whose
  // response could land in either order.
  const runningRef = useRef(false)

  async function run() {
    if (runningRef.current) return
    runningRef.current = true
    setBusy(true)
    try {
      const result = await api.run(slug, solutionId)
      setPayload(result)
      if (kind === 'Attempt' && result.run?.status === 'Passed') {
        markSolved(slug)
      }
    } finally {
      runningRef.current = false
      setBusy(false)
    }
  }

  return (
    <div className="mt-3">
      <button
        type="button"
        onClick={run}
        disabled={busy}
        className="rounded bg-blue-600 px-3 py-1 text-sm text-white disabled:opacity-50"
      >
        {busy ? 'Идёт запуск…' : 'Запустить'}
      </button>

      {payload?.error && <p className="mt-2 text-sm text-red-600">{payload.error}</p>}

      {payload?.run && (
        <div className="mt-3">
          <p className="mb-2 text-sm font-medium">{statusLabel(payload.run.status)}</p>
          <table className="w-full text-left text-xs">
            <thead className="text-slate-500">
              <tr>
                <th className="py-1">Кейс</th>
                <th>Вход</th>
                <th>Ожидание</th>
                <th>Факт</th>
                <th>мс</th>
              </tr>
            </thead>
            <tbody>
              {payload.run.cases.map((c) => (
                <tr key={c.name} className={c.passed ? '' : 'bg-red-50'}>
                  <td className="py-1">{c.name}</td>
                  <td className="font-mono">{c.input}</td>
                  <td className="font-mono">{c.expected}</td>
                  <td className="font-mono">{c.error ?? c.actual}</td>
                  <td>{c.elapsedMs.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
