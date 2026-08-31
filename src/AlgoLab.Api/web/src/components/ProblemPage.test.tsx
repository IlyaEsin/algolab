import { render, screen, waitFor } from '@testing-library/react'
import { beforeEach, expect, test, vi } from 'vitest'
import type { ProblemDetail } from '../api/types'
import { api } from '../api/client'
import { ProblemPage } from './ProblemPage'

vi.mock('../api/client', () => ({
  api: { getProblem: vi.fn() },
}))

function detail(slug: string, title: string): ProblemDetail {
  return {
    summary: { slug, title, difficulty: 'Easy', tags: [], hasScaler: false },
    source: 'https://example.com',
    readme: `Условие для ${title}.`,
    cases: [],
    solutions: [],
  }
}

beforeEach(() => {
  localStorage.clear()
  vi.mocked(api.getProblem).mockReset()
})

test('a stale response for an abandoned problem cannot overwrite the currently-selected one', async () => {
  // two-sum is already revealed from an earlier visit, before this render happens.
  localStorage.setItem('algolab.reveal.two-sum', '1')

  const resolvers: Array<(value: ProblemDetail) => void> = []
  const calls: string[] = []
  vi.mocked(api.getProblem).mockImplementation(
    (slug: string) =>
      new Promise<ProblemDetail>((resolve) => {
        calls.push(slug)
        resolvers.push(resolve)
      }),
  )

  const { rerender } = render(<ProblemPage slug="two-sum" />)
  expect(calls).toEqual(['two-sum'])
  resolvers[0](detail('two-sum', 'Сумма двух'))
  await screen.findByRole('heading', { level: 2, name: 'Сумма двух' })

  // user selects an unrevealed problem; its request is in flight
  rerender(<ProblemPage slug="valid-parentheses" />)
  await waitFor(() => expect(screen.getByText('Загрузка…')).not.toBeNull())
  expect(calls).toEqual(['two-sum', 'valid-parentheses'])

  // ...then selects two-sum again before that request resolves
  rerender(<ProblemPage slug="two-sum" />)
  expect(calls).toEqual(['two-sum', 'valid-parentheses', 'two-sum'])

  // the abandoned valid-parentheses request now resolves late
  resolvers[1](detail('valid-parentheses', 'Баланс скобок'))
  await new Promise((r) => setTimeout(r, 0))

  // it must not leak into view: we're on two-sum, and its own second request hasn't resolved yet
  expect(screen.queryByText('Баланс скобок')).toBeNull()
  expect(screen.getByText('Загрузка…')).not.toBeNull()

  // the current problem's own response arrives and is shown correctly
  resolvers[2](detail('two-sum', 'Сумма двух'))
  await screen.findByRole('heading', { level: 2, name: 'Сумма двух' })
  expect(screen.queryByText('Баланс скобок')).toBeNull()
})
