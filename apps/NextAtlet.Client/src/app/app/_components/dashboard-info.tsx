'use client';

import { useUser } from '@auth0/nextjs-auth0/client';

export const DashboardInfo = () => {
  const user = useUser();

  return (
    <>
      <h1 className="text-xl">
        Welcome <b>{`${user.user?.firstName} ${user.user?.lastName}`}</b>
      </h1>
    </>
  );
};
