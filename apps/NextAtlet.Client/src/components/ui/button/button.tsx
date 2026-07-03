import { Slot } from '@radix-ui/react-slot';
import { cva, type VariantProps } from 'class-variance-authority';
import * as React from 'react';

import { cn } from '@/utils/cn';

import { Spinner } from '../spinner';

const buttonVariants = cva(
  'inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        // Primary action = gold (the default CTA across the app).
        primary: 'btn-primary',
        destructive: 'btn-destructive',
        outline: 'btn-outline',
        outlineGold: 'btn-outline btn-outline-gold',
        outlineMuted: 'btn-outline btn-outline-muted',
        secondary: 'btn-secondary',
        ghost: 'btn-ghost',
        link: 'link-arrow',
        // Nordic Gold brand variants (dark marketing surfaces). Add new brand styles here so call
        // sites stay declarative — open for extension without touching the markup that uses them.

      },
      size: {
        primary: 'h-9 rounded-full px-4 py-2',
        sm: 'h-8 rounded-full px-3 text-xs',
        lg: 'h-10 rounded-full px-8',
        icon: 'rounded-full size-9',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'primary',
    },
  },
);

export type ButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement> &
  VariantProps<typeof buttonVariants> & {
    asChild?: boolean;
    isLoading?: boolean;
    icon?: React.ReactNode;
  };

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      className,
      variant,
      size,
      asChild = false,
      children,
      isLoading,
      icon,
      ...props
    },
    ref,
  ) => {
    const Comp = asChild ? Slot : 'button';
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      >
        {isLoading && <Spinner size="sm" className="text-current" />}
        <span className="mx-2">{children}</span>
        {!isLoading && icon && <span className="mr-2">{icon}</span>}
      </Comp>
    );
  },
);
Button.displayName = 'Button';

export { Button, buttonVariants };
