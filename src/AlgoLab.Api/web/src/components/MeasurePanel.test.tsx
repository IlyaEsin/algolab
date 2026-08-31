import { act, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, expect, test, vi } from 'vitest'
import type { SolutionView } from '../api/types'
import { MeasurePanel } from './MeasurePanel'

const solutions: SolutionView[] = [
  { id: 'TwoSumBruteForce', name: 'Перебор', kind: 'Reference', time: 'O(n^2)', space: 'O(1)', note: null, source: 'x' },
  { id: 'TwoSumHashMap', name: 'Хеш-таблица', kind: 'Reference', time: 'O(n)', space: 'O(n)', note: null, source: 'y' },
]

beforeEach(() => localStorage.clear())
afterEach(() => vi.restoreAllMocks())

test('a second click while a measurement is in flight does not start a concurrent run', async () => {
  let resolveFetch: (value: unknown) => void = () => {}
  const fetchMock = vi.fn().mockImplementation(
    () =>
      new Promise((resolve) => {
        resolveFetch = resolve
      }),
  )
  vi.stubGlobal('fetch', fetchMock)

  render(<MeasurePanel slug="two-sum" solutions={solutions} />)
  const button = screen.getByRole('button', { name: 'Снять кривые роста' })

  // Two raw click events dispatched inside one act() batch, so React has not yet
  // committed the `disabled` state between them — this reproduces a real fast
  // double-click, unlike two separate fireEvent.click() calls (which React's
  // synchronous act-flush already serializes on its own, masking a missing guard).
  act(() => {
    button.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }))
    button.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }))
  })

  expect(fetchMock).toHaveBeenCalledTimes(1)

  resolveFetch({
    ok: true,
    json: async () => ({
      run: null,
      series: [
        {
          slug: 'two-sum',
          solutionId: 'TwoSumHashMap',
          points: [{ n: 64, medianMs: 0.01, allocatedBytes: 32 }],
          time: { declared: 'ON', bestFit: 'ON', declaredText: 'O(n)', bestFitText: 'O(n)', fitQuality: 0.99, kind: 'Consistent' },
          space: { declared: 'ON', bestFit: 'ON', declaredText: 'O(n)', bestFitText: 'O(n)', fitQuality: 0.99, kind: 'Consistent' },
          error: null,
        },
      ],
      error: null,
    }),
  })

  await waitFor(() => expect(screen.getByText('TwoSumHashMap')).toBeInTheDocument())
  expect(fetchMock).toHaveBeenCalledTimes(1)
})
