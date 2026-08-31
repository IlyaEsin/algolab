import { afterEach, expect, test, vi } from 'vitest'
import { api } from './client'

afterEach(() => {
  vi.restoreAllMocks()
})

test('listProblems requests the problems endpoint', async () => {
  const fetchMock = vi.fn().mockResolvedValue({
    ok: true,
    json: async () => [{ slug: 'two-sum' }],
  })
  vi.stubGlobal('fetch', fetchMock)

  const problems = await api.listProblems()

  expect(fetchMock).toHaveBeenCalledWith('/api/problems')
  expect(problems[0].slug).toBe('two-sum')
})

test('run posts slug and solution id', async () => {
  const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => ({}) })
  vi.stubGlobal('fetch', fetchMock)

  await api.run('two-sum', 'TwoSumAttempt')

  const [url, init] = fetchMock.mock.calls[0]
  expect(url).toBe('/api/run')
  expect(JSON.parse(init.body)).toEqual({ slug: 'two-sum', solutionId: 'TwoSumAttempt' })
})

test('failed response throws with the status', async () => {
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 404 }))

  await expect(api.getProblem('nope')).rejects.toThrow('404')
})
