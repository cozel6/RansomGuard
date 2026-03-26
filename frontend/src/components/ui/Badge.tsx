// src/components/ui/Badge.tsx
import { forwardRef, HTMLAttributes } from 'react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/utils/cn'

const badgeVariants = cva(
  'inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-all',
  {
    variants: {
      variant: {
        default: 'border-transparent bg-terminal-green text-terminal-black shadow-glow-green',
        safe: 'border-terminal-green text-terminal-green shadow-glow-green',
        suspicious: 'border-terminal-yellow text-terminal-yellow',
        ransomware: 'border-terminal-red text-terminal-red shadow-glow-red animate-glow-pulse',
        secondary: 'border-terminal-light text-terminal-muted',
        outline: 'border-terminal-cyan text-terminal-cyan',
      },
    },
    defaultVariants: {
      variant: 'default',
    },
  }
)

export interface BadgeProps
  extends HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

const Badge = forwardRef<HTMLDivElement, BadgeProps>(
  ({ className, variant, ...props }, ref) => (
    <div ref={ref} className={cn(badgeVariants({ variant }), className)} {...props} />
  )
)

Badge.displayName = 'Badge'

export { Badge, badgeVariants }
