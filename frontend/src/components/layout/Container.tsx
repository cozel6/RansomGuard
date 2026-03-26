// src/components/layout/Container.tsx
import { HTMLAttributes } from 'react'
import { cn } from '@/utils/cn'

interface ContainerProps extends HTMLAttributes<HTMLDivElement> {
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full'
}

const sizeClasses = {
  sm: 'max-w-2xl',
  md: 'max-w-4xl',
  lg: 'max-w-6xl',
  xl: 'max-w-7xl',
  full: 'max-w-full',
}

export function Container({ size = 'lg', className, children, ...props }: ContainerProps) {
  return (
    <div className={cn('container mx-auto px-4', sizeClasses[size], className)} {...props}>
      {children}
    </div>
  )
}
