import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect, test, vi, afterEach, beforeEach } from 'vitest'
import { isSolved } from '../state/useReveal'
import { RunPanel, statusLabel } from './RunPanel'

beforeEach(() => localStorage.clear())
afterEach(() => vi.restoreAllMocks())

test('status labels are human readable', () => {
  expect(statusLabel('Passed')).toBe('Все кейсы пройдены')
  expect(statusLabel('NotImplemented')).toBe('Решение не реализовано')
})

test('shows a row per case after running', async () => {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        run: {
          slug: 'two-sum',
          solutionId: 'TwoSumHashMap',
          status: 'Passed',
          cases: [
            { name: 'первый', input: '{}', expected: '[0,1]', actual: '[0,1]', passed: true, elapsedMs: 0.1, error: null },
          ],
        },
        series: null,
        error: null,
      }),
    }),
  )

  render(<RunPanel slug="two-sum" solutionId="TwoSumHashMap" kind="Reference" />)
  await userEvent.click(screen.getByRole('button', { name: 'Запустить' }))

  await waitFor(() => expect(screen.getByText('первый')).toBeInTheDocument())
  expect(screen.getByText('Все кейсы пройдены')).toBeInTheDocument()
})

test('surfaces runner errors', async () => {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ run: null, series: null, error: 'таймаут' }),
    }),
  )

  render(<RunPanel slug="two-sum" solutionId="TwoSumAttempt" kind="Attempt" />)
  await userEvent.click(screen.getByRole('button', { name: 'Запустить' }))

  await waitFor(() => expect(screen.getByText('таймаут')).toBeInTheDocument())
})

test('a passing attempt marks the problem solved, a passing reference does not', async () => {
  const passing = {
    ok: true,
    json: async () => ({
      run: {
        slug: 'two-sum',
        solutionId: 'whatever',
        status: 'Passed',
        cases: [{ name: 'первый', input: '{}', expected: '[0,1]', actual: '[0,1]', passed: true, elapsedMs: 0.1, error: null }],
      },
      series: null,
      error: null,
    }),
  }

  vi.stubGlobal('fetch', vi.fn().mockResolvedValue(passing))
  render(<RunPanel slug="two-sum" solutionId="TwoSumHashMap" kind="Reference" />)
  await userEvent.click(screen.getByRole('button', { name: 'Запустить' }))
  await waitFor(() => expect(screen.getByText('Все кейсы пройдены')).toBeInTheDocument())
  expect(isSolved('two-sum')).toBe(false)

  vi.stubGlobal('fetch', vi.fn().mockResolvedValue(passing))
  render(<RunPanel slug="two-sum" solutionId="TwoSumAttempt" kind="Attempt" />)
  await userEvent.click(screen.getAllByRole('button', { name: 'Запустить' })[1])
  await waitFor(() => expect(isSolved('two-sum')).toBe(true))
})

test('a second click while a run is in flight does not start a concurrent run', async () => {
  let resolveFetch: (value: unknown) => void = () => {}
  const fetchMock = vi.fn().mockImplementation(
    () =>
      new Promise((resolve) => {
        resolveFetch = resolve
      }),
  )
  vi.stubGlobal('fetch', fetchMock)

  render(<RunPanel slug="two-sum" solutionId="TwoSumAttempt" kind="Attempt" />)
  const button = screen.getByRole('button', { name: 'Запустить' })

  // Two clicks dispatched back-to-back, before React has a chance to disable the button.
  fireEvent.click(button)
  fireEvent.click(button)

  expect(fetchMock).toHaveBeenCalledTimes(1)

  resolveFetch({
    ok: true,
    json: async () => ({
      run: { slug: 'two-sum', solutionId: 'TwoSumAttempt', status: 'Passed', cases: [] },
      series: null,
      error: null,
    }),
  })

  await waitFor(() => expect(screen.getByText('Все кейсы пройдены')).toBeInTheDocument())
  expect(fetchMock).toHaveBeenCalledTimes(1)
})

test('a failed rerun replaces the previous case table instead of showing both', async () => {
  const success = {
    ok: true,
    json: async () => ({
      run: {
        slug: 'two-sum',
        solutionId: 'TwoSumAttempt',
        status: 'Passed',
        cases: [{ name: 'первый', input: '{}', expected: '[0,1]', actual: '[0,1]', passed: true, elapsedMs: 0.1, error: null }],
      },
      series: null,
      error: null,
    }),
  }
  const failure = {
    ok: true,
    json: async () => ({ run: null, series: null, error: 'таймаут' }),
  }
  const fetchMock = vi.fn().mockResolvedValueOnce(success).mockResolvedValueOnce(failure)
  vi.stubGlobal('fetch', fetchMock)

  render(<RunPanel slug="two-sum" solutionId="TwoSumAttempt" kind="Attempt" />)
  const button = screen.getByRole('button', { name: 'Запустить' })

  await userEvent.click(button)
  await waitFor(() => expect(screen.getByText('первый')).toBeInTheDocument())

  await userEvent.click(button)
  await waitFor(() => expect(screen.getByText('таймаут')).toBeInTheDocument())
  expect(screen.queryByText('первый')).not.toBeInTheDocument()
})
