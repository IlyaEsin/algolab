import { CartesianGrid, Legend, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { GrowthSeries } from '../api/types'

const colors = ['#2563eb', '#dc2626', '#059669', '#d97706']

/** Ось X логарифмическая: только так разница между n и n^2 читается как разный наклон.
 * Ось Y — тоже, но лишь когда все значения строго положительны: allocatedBytes легитимно
 * равен нулю для O(1)-памяти, а логарифмическая шкала не может отобразить ноль. */
export function GrowthChart({ series, metric }: { series: GrowthSeries[]; metric: 'time' | 'space' }) {
  const sizes = [...new Set(series.flatMap((s) => s.points.map((p) => p.n)))].sort((a, b) => a - b)

  const rows = sizes.map((n) => {
    const row: Record<string, number> = { n }
    for (const s of series) {
      const point = s.points.find((p) => p.n === n)
      if (point) {
        row[s.solutionId] = metric === 'time' ? point.medianMs : point.allocatedBytes
      }
    }
    return row
  })

  const values = series.flatMap((s) => s.points.map((p) => (metric === 'time' ? p.medianMs : p.allocatedBytes)))
  const yScale = values.every((v) => v > 0) ? 'log' : 'linear'

  return (
    <ResponsiveContainer width="100%" height={320}>
      <LineChart data={rows}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="n" scale="log" domain={['auto', 'auto']} type="number" />
        <YAxis scale={yScale} domain={['auto', 'auto']} />
        <Tooltip />
        <Legend />
        {series.map((s, i) => (
          <Line key={s.solutionId} type="monotone" dataKey={s.solutionId} stroke={colors[i % colors.length]} dot />
        ))}
      </LineChart>
    </ResponsiveContainer>
  )
}
