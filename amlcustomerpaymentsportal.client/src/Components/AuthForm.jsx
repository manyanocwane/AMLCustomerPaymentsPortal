import React, { useState } from 'react';

const AuthForm = () => {
    const [isLogin] = useState(true); // Force login mode only, no toggle
    const [form, setForm] = useState({ email: '', password: '' });
    const [message, setMessage] = useState('');

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const route = isLogin ? 'login' : 'register'; // Will always be "login"

        try {
            const response = await fetch(`/api/user/${route}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(form),
            });

            const text = await response.text();
            setMessage(text);
        // eslint-disable-next-line no-unused-vars
        } catch (err) {
            setMessage('Something went wrong.');
        }
    };

    return (
        <div style={{ maxWidth: '400px', margin: 'auto' }}>
            <h2>Login</h2>
            <form onSubmit={handleSubmit}>
                <input
                    name="email"
                    type="email"
                    placeholder="Email"
                    value={form.email}
                    onChange={handleChange}
                    required
                /><br />
                <input
                    name="password"
                    type="password"
                    placeholder="Password"
                    value={form.password}
                    onChange={handleChange}
                    required
                /><br />
                <button type="submit">Login</button>
            </form>
            {/* Registration toggle removed */}
            <p>{message}</p>
        </div>
    );
};

export default AuthForm;
