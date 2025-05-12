// ProtectedRoute.jsx
import { Navigate, Outlet } from "react-router-dom";

const ProtectedRoute = ({ allowedRoles }) => {
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');

    if (!token) return <Navigate to="/login" replace />;

    if (!allowedRoles.includes(role)) {
        return <div>Доступ запрещен</div>;
    }

    return <Outlet />;
};

export default ProtectedRoute;