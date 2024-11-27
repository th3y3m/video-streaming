import ReactPlayer from 'react-player/vimeo';
import React, { useState } from 'react';
import axios from 'axios';

export const VideoPlayer = ({ videoUri }) => {
    if (!videoUri) {
        return <p>No video uploaded yet.</p>; // Show this before a video is available
    }

    return (
        <div>
            <ReactPlayer url={videoUri} controls />
        </div>
    );
};

export const VideoUpload = () => {
    const [videoFile, setVideoFile] = useState(null);
    const [videoTitle, setVideoTitle] = useState('');
    const [videoDescription, setVideoDescription] = useState('');
    const [videoUri, setVideoUri] = useState(null); // To store the uploaded video's URI
    const [error, setError] = useState(null); // To store any error messages

    const handleFileChange = (e) => {
        setVideoFile(e.target.files[0]);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        const formData = new FormData();
        formData.append('video', videoFile);
        formData.append('title', videoTitle);
        formData.append('description', videoDescription);

        try {
            const response = await axios.post('https://localhost:8080/api/videos/upload', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            });
            console.log('Video uploaded:', response.data);

            // Extract the video URI from the response and set it
            setVideoUri(response.data.url); // Assuming backend returns `videoUri`
            setError(null); // Clear any previous errors
        } catch (error) {
            console.error('Error uploading video:', error);
            setError('Error uploading video. Please try again.');
        }
    };

    return (
        <div>
            <form onSubmit={handleSubmit}>
                <input type="file" onChange={handleFileChange} />
                <input
                    type="text"
                    value={videoTitle}
                    onChange={(e) => setVideoTitle(e.target.value)}
                    placeholder="Title"
                />
                <textarea
                    value={videoDescription}
                    onChange={(e) => setVideoDescription(e.target.value)}
                    placeholder="Description"
                />
                <button type="submit">Upload Video</button>
            </form>
            {error && <p style={{ color: 'red' }}>{error}</p>}
            <VideoPlayer videoUri={videoUri} /> {/* Pass the video URI dynamically */}
        </div>
    );
};
