import { useRef, useState } from 'react'
import { api } from '../api/client'
import type { GrowthSeries, SolutionView } from '../api/types'
import { GrowthChart } from './GrowthChart'
import { VerdictBadge } from './VerdictBadge'

export function MeasurePanel({ slug, solutions }: { slug: string; solutions: SolutionView[] }) {
  const [series, setSeries] = useState<GrowthSeries[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [metric, setMetric] = useState<'time' | 'space'>('time')

  // See RunPanel: `disabled={busy}` alone can't close the double-click window, because
  // React applies that update asynchronously. This ref check is synchronous, so a second
  // click while a measurement is in flight is a no-op rather than a concurrent run whose
  // response could land in either order and overwrite a newer one.
  const measuringRef = useRef(false)

  async function measure() {
    if (measuringRef.current) return
    measuringRef.current = true
    setBusy(true)
    try {
      const payload = await api.measure(slug, solutions.map((s) => s.id))
      setSeries(payload.series)
      setError(payload.error)
    } finally {
      measuringRef.current = false
      setBusy(false)
    }
  }

  const measured = series?.filter((s) => s.points.length > 0) ?? []

  return (
    <section className="mt-6">
      <div className="mb-3 flex items-center gap-3">
        <button
          type="button"
          onClick={measure}
          disabled={busy}
          className="rounded bg-slate-800 px-3 py-1 text-sm text-white disabled:opacity-50"
        >
          {busy ? 'Идут замеры…' : 'Снять кривые роста'}
        </button>
        <select
          className="rounded border border-slate-300 px-2 py-1 text-sm"
          value={metric}
          onChange={(e) => setMetric(e.target.value as 'time' | 'space')}
        >
          <option value="time">время</option>
          <option value="space">память</option>
        </select>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      {measured.length > 0 && (
        <>
          <GrowthChart series={measured} metric={metric} />
          <ul className="mt-3 space-y-1">
            {measured.map((s) => (
              <li key={s.solutionId} className="text-sm">
                <span className="mr-2 font-medium">{s.solutionId}</span>
                <VerdictBadge verdict={metric === 'time' ? s.time : s.space} />
              </li>
            ))}
          </ul>
          <p className="mt-2 text-xs text-slate-500">
            Соседние классы (n и n log n) на достижимых размерах входа неразличимы — вердикт означает
            согласие с заявленным, а не доказательство.
          </p>
        </>
      )}
    </section>
  )
}
