import { expect, test } from 'vitest'
import { yAxisScale } from './GrowthChart'

test('all-positive values get a logarithmic scale', () => {
  expect(yAxisScale([1, 32, 8192])).toBe('log')
})

test('values containing a zero fall back to a linear scale', () => {
  expect(yAxisScale([0, 32, 8192])).toBe('linear')
})
