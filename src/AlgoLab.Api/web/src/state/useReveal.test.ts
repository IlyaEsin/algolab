import { act, renderHook } from '@testing-library/react'
import { beforeEach, expect, test } from 'vitest'
import { isSolved, markSolved, useReveal } from './useReveal'

beforeEach(() => {
  localStorage.clear()
})

test('solutions start hidden', () => {
  const { result } = renderHook(() => useReveal('two-sum'))

  expect(result.current.revealed).toBe(false)
})

test('reveal survives a remount', () => {
  const first = renderHook(() => useReveal('two-sum'))
  act(() => first.result.current.reveal())

  const second = renderHook(() => useReveal('two-sum'))

  expect(second.result.current.revealed).toBe(true)
})

test('reveal is per problem', () => {
  const first = renderHook(() => useReveal('two-sum'))
  act(() => first.result.current.reveal())

  const other = renderHook(() => useReveal('valid-parentheses'))

  expect(other.result.current.revealed).toBe(false)
})

test('hide clears the stored flag', () => {
  const { result } = renderHook(() => useReveal('two-sum'))
  act(() => result.current.reveal())
  act(() => result.current.hide())

  expect(renderHook(() => useReveal('two-sum')).result.current.revealed).toBe(false)
})

test('solved marks are stored per problem', () => {
  expect(isSolved('two-sum')).toBe(false)

  markSolved('two-sum')

  expect(isSolved('two-sum')).toBe(true)
  expect(isSolved('lru-cache')).toBe(false)
})
